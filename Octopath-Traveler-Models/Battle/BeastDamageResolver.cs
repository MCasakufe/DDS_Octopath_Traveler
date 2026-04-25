namespace Octopath_Traveler_Models.Battle;

public sealed record BeastDamageResolution(
    int Damage,
    int TargetCurrentHp,
    bool IsWeaknessHit,
    bool EnteredBreakingPoint);

public sealed record BeastHitRequest(
    int AttackerPhysAtk,
    int AttackerElemAtk,
    BeastCombatUnit Target,
    string DamageType,
    double SkillModifier);

public sealed class BeastDamageResolver
{
    private const double WeaknessDamageMultiplier = 1.5;
    private const double BreakingPointDamageMultiplier = 1.5;
    private const double WeaknessAndBreakingDamageMultiplier = 2.0;
    private const double NoBonusDamageMultiplier = 1.0;
    private const int ZeroDamage = 0;
    private const int NoShieldsRemaining = 0;
    private const int BreakingRoundsDuration = 2;

    private static readonly HashSet<string> PhysicalDamageTypes = new(StringComparer.Ordinal)
    {
        "Sword",
        "Spear",
        "Dagger",
        "Axe",
        "Bow",
        "Stave"
    };

    public BeastDamageResolution ResolveHit(
        BeastHitRequest hitRequest)
        => ResolveDamageCore(
            hitRequest,
            bonusDamageMultiplier: NoBonusDamageMultiplier,
            damageCap: DamageCapType.None);

    public BeastDamageResolution ResolveHitWithBonus(
        BeastHitRequest hitRequest,
        double bonusDamageMultiplier)
        => ResolveDamageCore(
            hitRequest,
            bonusDamageMultiplier,
            damageCap: DamageCapType.None);

    public BeastDamageResolution ResolveHitKeepingTargetAlive(
        BeastHitRequest hitRequest)
        => ResolveDamageCore(
            hitRequest,
            bonusDamageMultiplier: NoBonusDamageMultiplier,
            damageCap: DamageCapType.KeepAtLeastOneHp);

    private static BeastDamageResolution ResolveDamageCore(
        BeastHitRequest hitRequest,
        double bonusDamageMultiplier,
        DamageCapType damageCap)
    {
        BeastCombatUnit target = hitRequest.Target;
        bool isWeaknessHit = target.Weaknesses.Contains(hitRequest.DamageType);
        bool isTargetInBreakingStateBeforeHit = target.RemainingBreakingRounds > 0;

        int attackStat = IsPhysicalDamageType(hitRequest.DamageType)
            ? hitRequest.AttackerPhysAtk
            : hitRequest.AttackerElemAtk;
        int defenseStat = IsPhysicalDamageType(hitRequest.DamageType) ? target.PhysDef : target.ElemDef;

        double statusMultiplier = CalculateStatusDamageMultiplier(
            ResolveStatusDamageContext(new HitStatus(isWeaknessHit, isTargetInBreakingStateBeforeHit)));
        double rawDamage = (attackStat * hitRequest.SkillModifier - defenseStat) * statusMultiplier * bonusDamageMultiplier;
        int uncappedDamage = Math.Max(0, (int)Math.Floor(rawDamage));
        int damage = ApplyDamageCap(uncappedDamage, target.CurrentHp, damageCap);

        target.ReceiveDamage(damage);
        bool enteredBreakingPoint = TryEnterBreakingPoint(new BreakingPointAttempt(
            target,
            isWeaknessHit,
            damage,
            isTargetInBreakingStateBeforeHit));

        return new BeastDamageResolution(
            damage,
            target.CurrentHp,
            isWeaknessHit,
            enteredBreakingPoint);
    }

    private static int ApplyDamageCap(int uncappedDamage, int targetCurrentHp, DamageCapType damageCap)
    {
        if (damageCap != DamageCapType.KeepAtLeastOneHp)
            return uncappedDamage;

        int maximumAllowedDamage = Math.Max(0, targetCurrentHp - 1);
        return Math.Min(uncappedDamage, maximumAllowedDamage);
    }

    private static bool TryEnterBreakingPoint(
        BreakingPointAttempt breakingPointAttempt)
    {
        if (!CanEnterBreakingPoint(breakingPointAttempt))
            return false;

        BeastCombatUnit target = breakingPointAttempt.Target;
        return target.ConsumeShieldAndTryEnterBreakingPoint(BreakingRoundsDuration);
    }

    private static bool CanEnterBreakingPoint(BreakingPointAttempt breakingPointAttempt)
        => breakingPointAttempt.IsWeaknessHit
           && breakingPointAttempt.Damage > ZeroDamage
           && !breakingPointAttempt.WasTargetInBreakingState
           && breakingPointAttempt.Target.CurrentShields > NoShieldsRemaining;

    private static StatusDamageContext ResolveStatusDamageContext(
        HitStatus hitStatus)
    {
        if (hitStatus.IsWeaknessHit && hitStatus.IsTargetInBreakingState)
            return StatusDamageContext.WeaknessAndBreaking;

        if (hitStatus.IsWeaknessHit)
            return StatusDamageContext.WeaknessOnly;

        if (hitStatus.IsTargetInBreakingState)
            return StatusDamageContext.BreakingOnly;

        return StatusDamageContext.NoBonus;
    }

    private static double CalculateStatusDamageMultiplier(StatusDamageContext statusDamageContext)
    {
        if (statusDamageContext == StatusDamageContext.WeaknessAndBreaking)
            return WeaknessAndBreakingDamageMultiplier;

        if (statusDamageContext == StatusDamageContext.WeaknessOnly)
            return WeaknessDamageMultiplier;

        if (statusDamageContext == StatusDamageContext.BreakingOnly)
            return BreakingPointDamageMultiplier;

        return NoBonusDamageMultiplier;
    }

    private static bool IsPhysicalDamageType(string damageType)
        => PhysicalDamageTypes.Contains(damageType);

    private enum DamageCapType
    {
        None,
        KeepAtLeastOneHp
    }

    private sealed record BreakingPointAttempt(
        BeastCombatUnit Target,
        bool IsWeaknessHit,
        int Damage,
        bool WasTargetInBreakingState);

    private sealed record HitStatus(
        bool IsWeaknessHit,
        bool IsTargetInBreakingState);

    private enum StatusDamageContext
    {
        NoBonus,
        WeaknessOnly,
        BreakingOnly,
        WeaknessAndBreaking
    }
}

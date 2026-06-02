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
        BeastHitCalculation calculation = CalculateHitDamage(new BeastHitCalculationRequest(
            hitRequest,
            bonusDamageMultiplier,
            damageCap));
        ApplyHitDamage(hitRequest.Target, calculation.Damage);
        bool enteredBreakingPoint = ApplyBreakingPointEntryIfEligible(new BreakingPointAttempt(
            hitRequest.Target,
            calculation.IsWeaknessHit,
            calculation.Damage,
            calculation.WasTargetInBreakingState));

        return new BeastDamageResolution(
            calculation.Damage,
            hitRequest.Target.CurrentHp,
            calculation.IsWeaknessHit,
            enteredBreakingPoint);
    }

    private static BeastHitCalculation CalculateHitDamage(BeastHitCalculationRequest calculationRequest)
    {
        BeastHitRequest hitRequest = calculationRequest.HitRequest;
        bool isWeaknessHit = hitRequest.Target.Weaknesses.Contains(hitRequest.DamageType);
        bool wasTargetInBreakingState = hitRequest.Target.RemainingBreakingRounds > 0;
        int uncappedDamage = CalculateUncappedDamage(new BeastHitStatusCalculation(
            hitRequest,
            calculationRequest.BonusDamageMultiplier,
            new HitStatus(isWeaknessHit, wasTargetInBreakingState)));
        int damage = ApplyDamageCap(uncappedDamage, hitRequest.Target.CurrentHp, calculationRequest.DamageCap);

        return new BeastHitCalculation(damage, isWeaknessHit, wasTargetInBreakingState);
    }

    private static int CalculateUncappedDamage(BeastHitStatusCalculation statusCalculation)
    {
        BeastHitRequest hitRequest = statusCalculation.HitRequest;
        int attackStat = SelectAttackStat(hitRequest);
        int defenseStat = SelectDefenseStat(hitRequest);
        double statusMultiplier = CalculateStatusDamageMultiplier(
            ResolveStatusDamageContext(statusCalculation.HitStatus));
        double rawDamage = (attackStat * hitRequest.SkillModifier - defenseStat)
                           * statusMultiplier
                           * statusCalculation.BonusDamageMultiplier;
        return Math.Max(ZeroDamage, (int)Math.Floor(rawDamage));
    }

    private static void ApplyHitDamage(BeastCombatUnit target, int damage)
        => target.ReceiveDamage(damage);

    private static int ApplyDamageCap(int uncappedDamage, int targetCurrentHp, DamageCapType damageCap)
    {
        if (damageCap != DamageCapType.KeepAtLeastOneHp)
            return uncappedDamage;

        int maximumAllowedDamage = Math.Max(0, targetCurrentHp - 1);
        return Math.Min(uncappedDamage, maximumAllowedDamage);
    }

    private static bool ApplyBreakingPointEntryIfEligible(
        BreakingPointAttempt breakingPointAttempt)
    {
        if (!CanEnterBreakingPoint(breakingPointAttempt))
            return false;

        return EnterBreakingPoint(breakingPointAttempt.Target);
    }

    private static bool CanEnterBreakingPoint(BreakingPointAttempt breakingPointAttempt)
        => breakingPointAttempt.IsWeaknessHit
           && breakingPointAttempt.Damage > ZeroDamage
           && !breakingPointAttempt.WasTargetInBreakingState
           && breakingPointAttempt.Target.CurrentShields > NoShieldsRemaining;

    private static bool EnterBreakingPoint(BeastCombatUnit target)
    {
        target.ConsumeShield();
        if (!target.HasNoShieldsRemaining())
            return false;

        target.EnterBreakingPoint(BreakingRoundsDuration);
        return true;
    }

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

    private static int SelectAttackStat(BeastHitRequest hitRequest)
        => IsPhysicalDamageType(hitRequest.DamageType)
            ? hitRequest.AttackerPhysAtk
            : hitRequest.AttackerElemAtk;

    private static int SelectDefenseStat(BeastHitRequest hitRequest)
        => IsPhysicalDamageType(hitRequest.DamageType)
            ? hitRequest.Target.PhysDef
            : hitRequest.Target.ElemDef;

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

    private sealed record BeastHitCalculation(
        int Damage,
        bool IsWeaknessHit,
        bool WasTargetInBreakingState);

    private sealed record BeastHitCalculationRequest(
        BeastHitRequest HitRequest,
        double BonusDamageMultiplier,
        DamageCapType DamageCap);

    private sealed record BeastHitStatusCalculation(
        BeastHitRequest HitRequest,
        double BonusDamageMultiplier,
        HitStatus HitStatus);

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

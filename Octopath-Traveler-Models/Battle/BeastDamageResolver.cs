namespace Octopath_Traveler_Models.Battle;

public sealed record BeastDamageResolution(
    int Damage,
    int TargetCurrentHp,
    bool IsWeaknessHit,
    bool EnteredBreakingPoint);

public sealed class BeastDamageResolver
{
    private const double WeaknessDamageMultiplier = 1.5;
    private const double BreakingPointDamageMultiplier = 1.5;
    private const double WeaknessAndBreakingDamageMultiplier = 2.0;
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
        int attackerPhysAtk,
        int attackerElemAtk,
        BeastCombatUnit target,
        string damageType,
        double skillModifier)
        => ResolveHitCore(
            attackerPhysAtk,
            attackerElemAtk,
            target,
            damageType,
            skillModifier,
            bonusDamageMultiplier: 1.0,
            damageCap: DamageCapType.None);

    public BeastDamageResolution ResolveHitWithBonus(
        int attackerPhysAtk,
        int attackerElemAtk,
        BeastCombatUnit target,
        string damageType,
        double skillModifier,
        double bonusDamageMultiplier)
        => ResolveHitCore(
            attackerPhysAtk,
            attackerElemAtk,
            target,
            damageType,
            skillModifier,
            bonusDamageMultiplier,
            damageCap: DamageCapType.None);

    public BeastDamageResolution ResolveHitKeepingTargetAlive(
        int attackerPhysAtk,
        int attackerElemAtk,
        BeastCombatUnit target,
        string damageType,
        double skillModifier)
        => ResolveHitCore(
            attackerPhysAtk,
            attackerElemAtk,
            target,
            damageType,
            skillModifier,
            bonusDamageMultiplier: 1.0,
            damageCap: DamageCapType.KeepAtLeastOneHp);

    private static BeastDamageResolution ResolveHitCore(
        int attackerPhysAtk,
        int attackerElemAtk,
        BeastCombatUnit target,
        string damageType,
        double skillModifier,
        double bonusDamageMultiplier,
        DamageCapType damageCap)
    {
        bool isWeaknessHit = target.Weaknesses.Contains(damageType);
        bool isTargetBrokenBeforeHit = target.RemainingBreakingRounds > 0;

        int attackStat = IsPhysicalDamageType(damageType) ? attackerPhysAtk : attackerElemAtk;
        int defenseStat = IsPhysicalDamageType(damageType) ? target.PhysDef : target.ElemDef;

        double statusMultiplier = GetStatusDamageMultiplier(isWeaknessHit, isTargetBrokenBeforeHit);
        double rawDamage = (attackStat * skillModifier - defenseStat) * statusMultiplier * bonusDamageMultiplier;
        int uncappedDamage = Math.Max(0, (int)Math.Floor(rawDamage));
        int damage = ApplyDamageCap(uncappedDamage, target.CurrentHp, damageCap);

        target.CurrentHp = Math.Max(0, target.CurrentHp - damage);
        bool enteredBreakingPoint = TryEnterBreakingPoint(target, isWeaknessHit, damage, isTargetBrokenBeforeHit);

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
        BeastCombatUnit target,
        bool isWeaknessHit,
        int damage,
        bool wasAlreadyBroken)
    {
        if (!isWeaknessHit || damage == 0 || wasAlreadyBroken || target.CurrentShields <= 0)
            return false;

        target.CurrentShields -= 1;
        if (target.CurrentShields > 0)
            return false;

        target.CurrentShields = 0;
        target.RemainingBreakingRounds = BreakingRoundsDuration;
        target.HasRecoveryPriorityCurrentRound = false;
        return true;
    }

    private static double GetStatusDamageMultiplier(bool isWeaknessHit, bool isTargetBroken)
    {
        if (isWeaknessHit && isTargetBroken)
            return WeaknessAndBreakingDamageMultiplier;

        if (isWeaknessHit)
            return WeaknessDamageMultiplier;

        if (isTargetBroken)
            return BreakingPointDamageMultiplier;

        return 1.0;
    }

    private static bool IsPhysicalDamageType(string damageType)
        => PhysicalDamageTypes.Contains(damageType);

    private enum DamageCapType
    {
        None,
        KeepAtLeastOneHp
    }
}

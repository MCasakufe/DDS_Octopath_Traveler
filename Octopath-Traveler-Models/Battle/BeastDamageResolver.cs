namespace Octopath_Traveler_Models.Battle;

public sealed class BeastDamageResolver
{
    private const double WeaknessDamageMultiplier = 1.5;
    private const double BreakingPointDamageMultiplier = 1.5;
    private const double WeaknessAndBreakingDamageMultiplier = 2.0;
    private const double NoBonusDamageMultiplier = 1.0;
    private const int ZeroDamage = 0;
    private const int NoShieldsRemaining = 0;
    private const int BreakingRoundsDuration = 2;
    private const int NoBreakingRoundsRemaining = 0;
    private const int MinimumAllowedDamage = 0;
    private const int MinimumSurvivingHp = 1;
    private const double DamageFloorTolerance = 0.000000001;

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
        bool wasTargetInBreakingState = hitRequest.Target.RemainingBreakingRounds > NoBreakingRoundsRemaining;
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
        int attackStat = SelectAttackStat(hitRequest, statusCalculation.HitStatus.IsWeaknessHit);
        int defenseStat = SelectDefenseStat(hitRequest);
        double weaknessAndBreakingMultiplier = CalculateStatusDamageMultiplier(
            ResolveStatusDamageContext(statusCalculation.HitStatus));
        double attackerStatusMultiplier = SelectAttackerStatusMultiplier(hitRequest);
        double defenderStatusMultiplier = SelectDefenderStatusMultiplier(hitRequest);
        double rawDamage = (attackStat * hitRequest.SkillModifier - defenseStat)
                           * attackerStatusMultiplier
                           * defenderStatusMultiplier
                           * weaknessAndBreakingMultiplier
                           * statusCalculation.BonusDamageMultiplier;
        return Math.Max(ZeroDamage, (int)Math.Floor(rawDamage + DamageFloorTolerance));
    }

    private static void ApplyHitDamage(BeastCombatUnit target, int damage)
        => target.ReceiveDamage(damage);

    private static int ApplyDamageCap(int uncappedDamage, int targetCurrentHp, DamageCapType damageCap)
    {
        if (damageCap != DamageCapType.KeepAtLeastOneHp)
            return uncappedDamage;

        int maximumAllowedDamage = Math.Max(MinimumAllowedDamage, targetCurrentHp - MinimumSurvivingHp);
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

    private static int SelectAttackStat(BeastHitRequest hitRequest, bool isWeaknessHit)
        => IsPhysicalDamageType(hitRequest.DamageType)
            ? SelectPhysicalAttackStat(hitRequest, isWeaknessHit)
            : SelectElementalAttackStat(hitRequest, isWeaknessHit);

    private static int SelectPhysicalAttackStat(BeastHitRequest hitRequest, bool isWeaknessHit)
        => hitRequest.Attacker.PhysAtk + SelectSwappedWeaknessPhysicalAttackBonus(hitRequest, isWeaknessHit);

    private static int SelectElementalAttackStat(BeastHitRequest hitRequest, bool isWeaknessHit)
        => hitRequest.Attacker.ElemAtk + SelectSwappedWeaknessElementalAttackBonus(hitRequest, isWeaknessHit);

    private static int SelectSwappedWeaknessPhysicalAttackBonus(BeastHitRequest hitRequest, bool isWeaknessHit)
        => isWeaknessHit
           && hitRequest.Attacker is TravelerCombatUnit traveler
           && traveler.HasStatSwap
           && traveler.Name == "Primrose"
            ? traveler.PhysAtkPassiveBonus
            : ZeroDamage;

    private static int SelectSwappedWeaknessElementalAttackBonus(BeastHitRequest hitRequest, bool isWeaknessHit)
        => ZeroDamage;

    private static int SelectDefenseStat(BeastHitRequest hitRequest)
        => IsPhysicalDamageType(hitRequest.DamageType)
            ? hitRequest.Target.PhysDef
            : hitRequest.Target.ElemDef;

    private static double SelectAttackerStatusMultiplier(BeastHitRequest hitRequest)
        => IsPhysicalDamageType(hitRequest.DamageType)
            ? hitRequest.Attacker.GetPhysicalAttackDamageMultiplier()
            : hitRequest.Attacker.GetElementalAttackDamageMultiplier();

    private static double SelectDefenderStatusMultiplier(BeastHitRequest hitRequest)
        => IsPhysicalDamageType(hitRequest.DamageType)
            ? hitRequest.Target.GetPhysicalDefenseDamageMultiplier()
            : hitRequest.Target.GetElementalDefenseDamageMultiplier();

}

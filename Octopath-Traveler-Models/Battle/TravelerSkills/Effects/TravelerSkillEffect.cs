namespace Octopath_Traveler_Models.Battle;

internal abstract class TravelerSkillEffect
{
    private const int MinimumHealing = 0;
    private const int NoMissingHpPercentage = 0;
    private const int MinimumPositiveMaxHp = 0;
    private const int MissingHpToPercentageMultiplier = 100;

    private static readonly BeastDamageResolver BeastDamageResolver = new();
    private static readonly TravelerSkillBoostCalculator SkillBoostCalculator = new();

    public abstract void Apply(TravelerSkillEffectContext effectContext);

    protected static BeastDamageResolution ResolveStandardBeastDamage(
        TravelerSkillEffectContext effectContext,
        BeastCombatUnit target,
        TravelerSkillDamageProfile damageProfile)
    {
        BeastHitRequest hitRequest = BuildBeastHitRequest(effectContext, target, damageProfile);
        return BeastDamageResolver.ResolveHit(hitRequest);
    }

    protected static BeastDamageResolution ResolveBeastDamageKeepingTargetAlive(
        TravelerSkillEffectContext effectContext,
        BeastCombatUnit target,
        TravelerSkillDamageProfile damageProfile)
    {
        BeastHitRequest hitRequest = BuildBeastHitRequest(effectContext, target, damageProfile);
        return BeastDamageResolver.ResolveHitKeepingTargetAlive(hitRequest);
    }

    protected static BeastDamageResolution ResolveBonusBeastDamage(
        TravelerSkillEffectContext effectContext,
        BeastCombatUnit target,
        TravelerSkillDamageBonusProfile damageBonusProfile)
    {
        BeastHitRequest hitRequest = BuildBeastHitRequest(
            effectContext,
            target,
            damageBonusProfile.DamageProfile);
        return BeastDamageResolver.ResolveHitWithBonus(hitRequest, damageBonusProfile.BonusMultiplier);
    }

    protected static void AddBeastDamageResultLines(
        TravelerSkillEffectContext effectContext,
        BeastCombatUnit target,
        string damageType,
        BeastDamageResolution damageResolution)
    {
        effectContext.AddResult(new TravelerSkillDamageResult(
            target.Name,
            damageResolution.Damage,
            damageType,
            damageResolution.IsWeaknessHit));
        if (damageResolution.EnteredBreakingPoint)
            effectContext.AddResult(new TravelerSkillBreakingPointResult(target.Name));
    }

    protected static void AddCurrentHpLines<TUnit>(
        TravelerSkillEffectContext effectContext,
        IEnumerable<TUnit> targets)
        where TUnit : Unit
    {
        foreach (TUnit target in targets)
            effectContext.AddResult(new TravelerSkillHpSummaryResult(target.Name, target.CurrentHp));
    }

    protected static void ApplyStatusEffectAndAddResult(
        TravelerSkillEffectContext effectContext,
        Unit target,
        UnitStatusEffectKind statusEffectKind,
        int durationRounds)
    {
        int appliedDurationRounds = target.ApplyStatusEffectAndReturnDuration(
            statusEffectKind,
            durationRounds,
            effectContext.Traveler);
        effectContext.AddResult(new TravelerSkillStatusEffectResult(
            target.Name,
            statusEffectKind,
            appliedDurationRounds));
    }

    protected static int CalculateHealing(TravelerSkillEffectContext effectContext)
    {
        double rawHealing = Math.Floor(CalculateRawHealing(effectContext));
        return Math.Max(MinimumHealing, (int)rawHealing);
    }

    protected static double CalculateRawHealing(TravelerSkillEffectContext effectContext)
        => effectContext.Traveler.ElemDef * CalculateBoostedModifier(effectContext);

    protected static int CalculateMissingHpPercentage(TravelerCombatUnit traveler)
    {
        if (traveler.MaxHp <= MinimumPositiveMaxHp || traveler.CurrentHp >= traveler.MaxHp)
            return NoMissingHpPercentage;

        int missingHp = traveler.MaxHp - traveler.CurrentHp;
        return (int)Math.Floor(missingHp * MissingHpToPercentageMultiplier / (double)traveler.MaxHp);
    }

    protected static TravelerSkillDamageProfile BuildSkillDamageProfile(TravelerSkillEffectContext effectContext)
        => BuildSkillDamageProfile(effectContext, effectContext.Skill.Type);

    protected static TravelerSkillDamageProfile BuildSkillDamageProfile(
        TravelerSkillEffectContext effectContext,
        string damageType)
        => new(damageType, CalculateBoostedModifier(effectContext));

    protected static int CalculateBoostedDuration(
        TravelerSkillEffectContext effectContext,
        int baseDurationRounds)
        => SkillBoostCalculator.CalculateBoostedDuration(
            effectContext.Skill,
            baseDurationRounds,
            effectContext.TurnOutcome.UsedBp);

    protected static int ApplyStandardSkillHitsAndSumDamage(
        TravelerSkillEffectContext effectContext,
        BeastCombatUnit target,
        TravelerSkillHitCountResolver hitCountResolver)
    {
        TravelerSkillDamageProfile damageProfile = BuildSkillDamageProfile(effectContext);
        int totalDamage = 0;
        int hitCount = hitCountResolver.ResolveHitCount(effectContext.Skill);
        for (int hitIndex = 0; hitIndex < hitCount; hitIndex++)
            totalDamage += ApplyStandardDamageAndReturnDamage(effectContext, target, damageProfile);

        return totalDamage;
    }

    private static double CalculateBoostedModifier(TravelerSkillEffectContext effectContext)
        => SkillBoostCalculator.CalculateBoostedModifier(
            effectContext.Skill,
            effectContext.TurnOutcome.UsedBp);

    private static int ApplyStandardDamageAndReturnDamage(
        TravelerSkillEffectContext effectContext,
        BeastCombatUnit target,
        TravelerSkillDamageProfile damageProfile)
    {
        BeastDamageResolution damageResolution = ResolveStandardBeastDamage(
            effectContext,
            target,
            damageProfile);
        AddBeastDamageResultLines(effectContext, target, damageProfile.DamageType, damageResolution);
        return damageResolution.Damage;
    }

    private static BeastHitRequest BuildBeastHitRequest(
        TravelerSkillEffectContext effectContext,
        BeastCombatUnit target,
        TravelerSkillDamageProfile damageProfile)
        => new(
            effectContext.Traveler,
            target,
            damageProfile.DamageType,
            damageProfile.Modifier);
}

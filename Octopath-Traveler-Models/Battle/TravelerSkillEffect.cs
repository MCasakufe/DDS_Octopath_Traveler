namespace Octopath_Traveler_Models.Battle;

internal abstract class TravelerSkillEffect
{
    private const int MissingHpToPercentageMultiplier = 100;

    private static readonly BeastDamageResolver BeastDamageResolver = new();

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

    protected static int CalculateHealing(TravelerSkillEffectContext effectContext)
    {
        double rawHealing = Math.Floor(effectContext.Traveler.ElemDef * effectContext.Skill.Modifier);
        return Math.Max(0, (int)rawHealing);
    }

    protected static int CalculateMissingHpPercentage(TravelerCombatUnit traveler)
    {
        if (traveler.MaxHp <= 0 || traveler.CurrentHp >= traveler.MaxHp)
            return 0;

        int missingHp = traveler.MaxHp - traveler.CurrentHp;
        return (int)Math.Floor(missingHp * MissingHpToPercentageMultiplier / (double)traveler.MaxHp);
    }

    protected static TravelerSkillDamageProfile BuildSkillDamageProfile(TravelerSkillEffectContext effectContext)
        => new(effectContext.Skill.Type, effectContext.Skill.Modifier);

    private static BeastHitRequest BuildBeastHitRequest(
        TravelerSkillEffectContext effectContext,
        BeastCombatUnit target,
        TravelerSkillDamageProfile damageProfile)
        => new(
            effectContext.Traveler.PhysAtk,
            effectContext.Traveler.ElemAtk,
            target,
            damageProfile.DamageType,
            damageProfile.Modifier);

}

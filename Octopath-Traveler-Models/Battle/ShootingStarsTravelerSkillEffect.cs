namespace Octopath_Traveler_Models.Battle;

internal sealed class ShootingStarsTravelerSkillEffect : TravelerSkillEffect
{
    private readonly IReadOnlyList<string> _damageTypes;

    public ShootingStarsTravelerSkillEffect(IReadOnlyList<string> damageTypes)
    {
        _damageTypes = damageTypes;
    }

    public override void Apply(TravelerSkillEffectContext effectContext)
    {
        IReadOnlyList<BeastCombatUnit> targets = effectContext.TargetSelection.BeastTargets;
        if (targets.Count == 0)
            return;

        foreach (BeastCombatUnit target in targets)
            ApplyDamageTypes(effectContext, target);

        AddCurrentHpLines(effectContext, targets);
    }

    private void ApplyDamageTypes(TravelerSkillEffectContext effectContext, BeastCombatUnit target)
    {
        foreach (string damageType in _damageTypes)
            ApplyDamage(effectContext, target, damageType);
    }

    private static void ApplyDamage(
        TravelerSkillEffectContext effectContext,
        BeastCombatUnit target,
        string damageType)
    {
        TravelerSkillDamageProfile damageProfile = new(damageType, effectContext.Skill.Modifier);
        BeastDamageResolution damageResolution = ResolveStandardBeastDamage(
            effectContext,
            target,
            damageProfile);
        AddBeastDamageResultLines(effectContext, target, damageType, damageResolution);
    }
}

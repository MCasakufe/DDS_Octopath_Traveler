namespace Octopath_Traveler_Models.Battle;

internal sealed class BeastDamageTravelerSkillEffect : TravelerSkillEffect
{
    private static readonly TravelerSkillHitCountResolver HitCountResolver = new();

    public override void Apply(TravelerSkillEffectContext effectContext)
    {
        IReadOnlyList<BeastCombatUnit> targets = effectContext.TargetSelection.BeastTargets;
        if (targets.Count == 0)
            return;

        TravelerSkillDamageProfile damageProfile = BuildSkillDamageProfile(effectContext);
        int hitCount = HitCountResolver.ResolveHitCount(effectContext.Skill);
        foreach (BeastCombatUnit target in targets)
            ApplySkillHits(effectContext, target, damageProfile, hitCount);

        AddCurrentHpLines(effectContext, targets);
    }

    private static void ApplySkillHits(
        TravelerSkillEffectContext effectContext,
        BeastCombatUnit target,
        TravelerSkillDamageProfile damageProfile,
        int hitCount)
    {
        for (int hitIndex = 0; hitIndex < hitCount; hitIndex++)
            ApplyDamage(effectContext, target, damageProfile);
    }

    private static void ApplyDamage(
        TravelerSkillEffectContext effectContext,
        BeastCombatUnit target,
        TravelerSkillDamageProfile damageProfile)
    {
        BeastDamageResolution damageResolution = ResolveStandardBeastDamage(
            effectContext,
            target,
            damageProfile);
        AddBeastDamageResultLines(effectContext, target, damageProfile.DamageType, damageResolution);
    }
}

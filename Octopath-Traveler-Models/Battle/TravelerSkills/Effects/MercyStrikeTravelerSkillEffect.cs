namespace Octopath_Traveler_Models.Battle;

internal sealed class MercyStrikeTravelerSkillEffect : TravelerSkillEffect
{
    public override void Apply(TravelerSkillEffectContext effectContext)
    {
        IReadOnlyList<BeastCombatUnit> targets = effectContext.TargetSelection.BeastTargets;
        if (targets.Count == 0)
            return;

        TravelerSkillDamageProfile damageProfile = BuildSkillDamageProfile(effectContext);
        foreach (BeastCombatUnit target in targets)
            ApplySkillActivations(effectContext, target, damageProfile);

        AddCurrentHpLines(effectContext, targets);
    }

    private static void ApplySkillActivations(
        TravelerSkillEffectContext effectContext,
        BeastCombatUnit target,
        TravelerSkillDamageProfile damageProfile)
    {
        for (int activationIndex = 0; activationIndex < effectContext.NonDivineSkillActivationCount; activationIndex++)
            ApplyDamage(effectContext, target, damageProfile);
    }

    private static void ApplyDamage(
        TravelerSkillEffectContext effectContext,
        BeastCombatUnit target,
        TravelerSkillDamageProfile damageProfile)
    {
        BeastDamageResolution damageResolution = ResolveBeastDamageKeepingTargetAlive(
            effectContext,
            target,
            damageProfile);
        AddBeastDamageResultLines(effectContext, target, damageProfile.DamageType, damageResolution);
    }
}

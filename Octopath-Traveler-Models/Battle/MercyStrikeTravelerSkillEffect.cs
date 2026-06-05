namespace Octopath_Traveler_Models.Battle;

internal sealed class MercyStrikeTravelerSkillEffect : TravelerSkillEffect
{
    public override void Apply(TravelerSkillEffectContext effectContext)
    {
        BeastCombatUnit? target = effectContext.TargetSelection.SingleBeastTarget;
        if (target is null)
            return;

        TravelerSkillDamageProfile damageProfile = BuildSkillDamageProfile(effectContext);
        for (int activationIndex = 0; activationIndex < effectContext.NonDivineSkillActivationCount; activationIndex++)
            ApplyDamage(effectContext, target, damageProfile);

        AddCurrentHpLines(effectContext, [target]);
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

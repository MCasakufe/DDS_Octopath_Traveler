namespace Octopath_Traveler_Models.Battle;

internal sealed class MercyStrikeTravelerSkillEffect : TravelerSkillEffect
{
    public override void Apply(TravelerSkillEffectContext effectContext)
    {
        BeastCombatUnit? target = effectContext.TargetSelection.SingleBeastTarget;
        if (target is null)
            return;

        TravelerSkillDamageProfile damageProfile = BuildSkillDamageProfile(effectContext);
        BeastDamageResolution damageResolution = ResolveBeastDamageKeepingTargetAlive(
            effectContext,
            target,
            damageProfile);
        AddBeastDamageResultLines(effectContext, target, damageProfile.DamageType, damageResolution);
        AddCurrentHpLines(effectContext, [target]);
    }
}

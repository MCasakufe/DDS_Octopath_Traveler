namespace Octopath_Traveler_Models.Battle;

internal sealed class SelectedWeaponBeastDamageTravelerSkillEffect : TravelerSkillEffect
{
    public override void Apply(TravelerSkillEffectContext effectContext)
    {
        BeastCombatUnit? target = effectContext.TargetSelection.SingleBeastTarget;
        if (target is null || string.IsNullOrEmpty(effectContext.TurnOutcome.SelectedWeapon))
            return;

        string weaponType = effectContext.TurnOutcome.SelectedWeapon;
        TravelerSkillDamageProfile damageProfile = BuildSkillDamageProfile(effectContext, weaponType);
        BeastDamageResolution damageResolution = ResolveStandardBeastDamage(
            effectContext,
            target,
            damageProfile);
        AddBeastDamageResultLines(effectContext, target, weaponType, damageResolution);
        AddCurrentHpLines(effectContext, [target]);
    }
}

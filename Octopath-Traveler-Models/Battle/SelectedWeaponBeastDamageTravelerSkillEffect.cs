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
        for (int activationIndex = 0; activationIndex < effectContext.NonDivineSkillActivationCount; activationIndex++)
            ApplyDamage(effectContext, target, weaponType, damageProfile);

        AddCurrentHpLines(effectContext, [target]);
    }

    private static void ApplyDamage(
        TravelerSkillEffectContext effectContext,
        BeastCombatUnit target,
        string weaponType,
        TravelerSkillDamageProfile damageProfile)
    {
        BeastDamageResolution damageResolution = ResolveStandardBeastDamage(
            effectContext,
            target,
            damageProfile);
        AddBeastDamageResultLines(effectContext, target, weaponType, damageResolution);
    }
}

namespace Octopath_Traveler_Models.Battle;

internal sealed class ElementalBreakTravelerSkillEffect : TravelerSkillEffect
{
    private const int StatusDurationRounds = 2;

    public override void Apply(TravelerSkillEffectContext effectContext)
    {
        BeastCombatUnit? target = effectContext.TargetSelection.SingleBeastTarget;
        if (target is null)
            return;

        TravelerSkillDamageProfile damageProfile = BuildSkillDamageProfile(effectContext);
        BeastDamageResolution damageResolution = ResolveStandardBeastDamage(
            effectContext,
            target,
            damageProfile);
        AddBeastDamageResultLines(effectContext, target, damageProfile.DamageType, damageResolution);
        ApplyStatusEffectAndAddResult(
            effectContext,
            target,
            UnitStatusEffectKind.DecreasedElementalDefense,
            StatusDurationRounds);
        AddCurrentHpLines(effectContext, [target]);
    }
}

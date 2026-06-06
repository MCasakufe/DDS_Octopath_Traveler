namespace Octopath_Traveler_Models.Battle;

internal sealed class ElementalBreakTravelerSkillEffect : TravelerSkillEffect
{
    private const int StatusDurationRounds = 2;

    public override void Apply(TravelerSkillEffectContext effectContext)
    {
        BeastCombatUnit? target = effectContext.TargetSelection.SingleBeastTarget;
        if (target is null)
            return;

        for (int activationIndex = 0; activationIndex < effectContext.NonDivineSkillActivationCount; activationIndex++)
            ApplyElementalBreak(effectContext, target);

        AddCurrentHpLines(effectContext, [target]);
    }

    private static void ApplyElementalBreak(
        TravelerSkillEffectContext effectContext,
        BeastCombatUnit target)
    {
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
    }
}

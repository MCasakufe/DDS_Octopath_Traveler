namespace Octopath_Traveler_Models.Battle;

internal sealed class LastStandTravelerSkillEffect : TravelerSkillEffect
{
    private readonly double _baseMultiplier;
    private readonly double _missingHpMultiplierPerPercent;

    public LastStandTravelerSkillEffect(double baseMultiplier, double missingHpMultiplierPerPercent)
    {
        _baseMultiplier = baseMultiplier;
        _missingHpMultiplierPerPercent = missingHpMultiplierPerPercent;
    }

    public override void Apply(TravelerSkillEffectContext effectContext)
    {
        IReadOnlyList<BeastCombatUnit> targets = effectContext.TargetSelection.BeastTargets;
        if (targets.Count == 0)
            return;

        TravelerSkillDamageBonusProfile damageBonusProfile = BuildDamageBonusProfile(effectContext);
        foreach (BeastCombatUnit target in targets)
            ApplyDamageActivations(effectContext, target, damageBonusProfile);

        AddCurrentHpLines(effectContext, targets);
    }

    private TravelerSkillDamageBonusProfile BuildDamageBonusProfile(TravelerSkillEffectContext effectContext)
    {
        int missingHpPercentage = CalculateMissingHpPercentage(effectContext.Traveler);
        double bonusMultiplier = _baseMultiplier + missingHpPercentage * _missingHpMultiplierPerPercent;
        return new(BuildSkillDamageProfile(effectContext), bonusMultiplier);
    }

    private static void ApplyDamage(
        TravelerSkillEffectContext effectContext,
        BeastCombatUnit target,
        TravelerSkillDamageBonusProfile damageBonusProfile)
    {
        BeastDamageResolution damageResolution = ResolveBonusBeastDamage(
            effectContext,
            target,
            damageBonusProfile);
        AddBeastDamageResultLines(
            effectContext,
            target,
            damageBonusProfile.DamageProfile.DamageType,
            damageResolution);
    }

    private static void ApplyDamageActivations(
        TravelerSkillEffectContext effectContext,
        BeastCombatUnit target,
        TravelerSkillDamageBonusProfile damageBonusProfile)
    {
        for (int activationIndex = 0; activationIndex < effectContext.NonDivineSkillActivationCount; activationIndex++)
            ApplyDamage(effectContext, target, damageBonusProfile);
    }
}

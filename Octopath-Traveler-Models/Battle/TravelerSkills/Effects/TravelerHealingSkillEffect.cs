namespace Octopath_Traveler_Models.Battle;

internal sealed class TravelerHealingSkillEffect : TravelerSkillEffect
{
    public override void Apply(TravelerSkillEffectContext effectContext)
    {
        IReadOnlyList<TravelerCombatUnit> targets = effectContext.TargetSelection.TravelerTargets;
        if (targets.Count == 0)
            return;

        int healedValue = CalculateHealing(effectContext);
        double rawHealingValue = CalculateRawHealing(effectContext);
        foreach (TravelerCombatUnit target in targets)
            ApplyHealingActivations(effectContext, target, healedValue, rawHealingValue);

        AddCurrentHpLines(effectContext, targets);
    }

    private static void ApplyHealingActivations(
        TravelerSkillEffectContext effectContext,
        TravelerCombatUnit target,
        int healedValue,
        double rawHealingValue)
    {
        for (int activationIndex = 0; activationIndex < effectContext.NonDivineSkillActivationCount; activationIndex++)
            ApplyHealing(effectContext, target, healedValue, rawHealingValue);
    }

    private static void ApplyHealing(
        TravelerSkillEffectContext effectContext,
        TravelerCombatUnit target,
        int healedValue,
        double rawHealingValue)
    {
        int appliedHealing = target.CalculateReceivedHealing(rawHealingValue, healedValue);
        target.RecoverHp(appliedHealing);
        effectContext.AddResult(new TravelerSkillHealingResult(target.Name, appliedHealing));
    }
}

namespace Octopath_Traveler_Models.Battle;

internal sealed class TravelerHealingSkillEffect : TravelerSkillEffect
{
    public override void Apply(TravelerSkillEffectContext effectContext)
    {
        IReadOnlyList<TravelerCombatUnit> targets = effectContext.TargetSelection.TravelerTargets;
        if (targets.Count == 0)
            return;

        int healedValue = CalculateHealing(effectContext);
        foreach (TravelerCombatUnit target in targets)
            ApplyHealing(effectContext, target, healedValue);

        AddCurrentHpLines(effectContext, targets);
    }

    private static void ApplyHealing(
        TravelerSkillEffectContext effectContext,
        TravelerCombatUnit target,
        int healedValue)
    {
        target.RecoverHp(healedValue);
        effectContext.AddResult(new TravelerSkillHealingResult(target.Name, healedValue));
    }
}

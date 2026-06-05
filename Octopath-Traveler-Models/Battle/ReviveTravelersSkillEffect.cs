namespace Octopath_Traveler_Models.Battle;

internal sealed class ReviveTravelersSkillEffect : TravelerSkillEffect
{
    private readonly int _reviveStartingHp;

    public ReviveTravelersSkillEffect(int reviveStartingHp)
    {
        _reviveStartingHp = reviveStartingHp;
    }

    public override void Apply(TravelerSkillEffectContext effectContext)
    {
        IReadOnlyList<TravelerCombatUnit> targets = effectContext.TargetSelection.TravelerTargets;
        if (targets.Count == 0)
            return;

        foreach (TravelerCombatUnit target in targets)
            ReviveTraveler(effectContext, target);

        AddCurrentHpLines(effectContext, targets);
    }

    private void ReviveTraveler(TravelerSkillEffectContext effectContext, TravelerCombatUnit target)
    {
        target.ReviveForNextRound(_reviveStartingHp);
        effectContext.AddResult(new TravelerSkillReviveResult(target.Name));
        ApplyBoostedHealing(effectContext, target);
    }

    private static void ApplyBoostedHealing(TravelerSkillEffectContext effectContext, TravelerCombatUnit target)
    {
        int healedValue = CalculateReviveHealing(effectContext);
        if (healedValue <= 0)
            return;

        int appliedHealing = target.CalculateReceivedHealing(healedValue);
        target.RecoverHp(appliedHealing);
        effectContext.AddResult(new TravelerSkillHealingResult(target.Name, appliedHealing));
    }

    private static int CalculateReviveHealing(TravelerSkillEffectContext effectContext)
    {
        double rawHealing = effectContext.Traveler.ElemDef
                            * effectContext.Skill.Modifier
                            * effectContext.TurnOutcome.UsedBp;
        return Math.Max(0, (int)Math.Floor(rawHealing));
    }
}

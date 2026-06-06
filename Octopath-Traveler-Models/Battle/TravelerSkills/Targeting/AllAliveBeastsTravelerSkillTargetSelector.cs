namespace Octopath_Traveler_Models.Battle;

internal sealed class AllAliveBeastsTravelerSkillTargetSelector : TravelerSkillTargetSelector
{
    public override TravelerSkillTargetSelection SelectTargets(TravelerSkillTargetSelectionContext selectionContext)
    {
        List<BeastCombatUnit> targets = selectionContext.BattleState.BeastTeam
            .Where(target => target.IsAlive)
            .OrderBy(target => target.BoardSlotIndex)
            .ToList();
        return TravelerSkillTargetSelection.WithBeasts(targets);
    }
}

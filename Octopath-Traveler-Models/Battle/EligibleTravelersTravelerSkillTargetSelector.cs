namespace Octopath_Traveler_Models.Battle;

internal abstract class EligibleTravelersTravelerSkillTargetSelector : TravelerSkillTargetSelector
{
    public sealed override TravelerSkillTargetSelection SelectTargets(TravelerSkillTargetSelectionContext selectionContext)
    {
        List<TravelerCombatUnit> targets = SelectEligibleTravelers(selectionContext).ToList();
        return TravelerSkillTargetSelection.WithTravelers(targets);
    }

    protected abstract bool IsEligibleTraveler(TravelerCombatUnit traveler);

    private IEnumerable<TravelerCombatUnit> SelectEligibleTravelers(TravelerSkillTargetSelectionContext selectionContext)
    {
        IEnumerable<TravelerCombatUnit> eligibleTravelers = selectionContext.BattleState.TravelerTeam
            .Where(IsEligibleTraveler);
        return OrderTargetsByBoardWithUserLast(eligibleTravelers, selectionContext.Traveler.BoardSlotIndex);
    }

    private static IReadOnlyList<TravelerCombatUnit> OrderTargetsByBoardWithUserLast(
        IEnumerable<TravelerCombatUnit> targets,
        int userBoardSlotIndex)
    {
        List<TravelerCombatUnit> orderedTargets = targets.OrderBy(target => target.BoardSlotIndex).ToList();
        List<TravelerCombatUnit> nonUserTargets = orderedTargets
            .Where(target => target.BoardSlotIndex != userBoardSlotIndex)
            .ToList();
        List<TravelerCombatUnit> userTargets = orderedTargets
            .Where(target => target.BoardSlotIndex == userBoardSlotIndex)
            .ToList();
        nonUserTargets.AddRange(userTargets);
        return nonUserTargets;
    }
}

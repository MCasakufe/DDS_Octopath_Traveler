namespace Octopath_Traveler_Models.Battle;

internal sealed class BeastAttackTargetSelector
{
    private const string EnemiesTargetType = "Enemies";
    private const int NoAliveTravelers = 0;

    private static readonly IReadOnlyList<BeastSingleTargetSelector> SingleTargetSelectors =
        BuildSingleTargetSelectors();

    private static readonly BeastSingleTargetSelector DefaultSingleTargetSelector =
        new HighestCurrentHpBeastSingleTargetSelector();

    public IReadOnlyList<TravelerCombatUnit> SelectTargets(BeastAttackTargetSelectionRequest selectionRequest)
    {
        List<TravelerCombatUnit> aliveTravelers = SelectAliveTravelers(selectionRequest.BattleState).ToList();
        if (aliveTravelers.Count == NoAliveTravelers)
            return [];

        if (selectionRequest.TargetType == EnemiesTargetType)
            return aliveTravelers.OrderBy(traveler => traveler.BoardSlotIndex).ToList();

        TravelerCombatUnit? selectedTarget = SelectSingleTarget(selectionRequest.SkillName, aliveTravelers);
        return selectedTarget is null ? [] : [selectedTarget];
    }

    private static TravelerCombatUnit? SelectSingleTarget(
        string skillName,
        IReadOnlyList<TravelerCombatUnit> aliveTravelers)
    {
        BeastSingleTargetSelector selector = SelectSingleTargetSelector(skillName);
        return selector.SelectTarget(aliveTravelers);
    }

    private static IReadOnlyList<BeastSingleTargetSelector> BuildSingleTargetSelectors()
        =>
        [
            new HighestElemAtkBeastSingleTargetSelector(),
            new LowestPhysDefBeastSingleTargetSelector(),
            new HighestPhysDefBeastSingleTargetSelector(),
            new HighestSpeedBeastSingleTargetSelector(),
            new LowestElemDefBeastSingleTargetSelector()
        ];

    private static BeastSingleTargetSelector SelectSingleTargetSelector(string skillName)
        => SingleTargetSelectors.FirstOrDefault(selector => selector.CanSelectTargetFor(skillName))
           ?? DefaultSingleTargetSelector;

    private static IEnumerable<TravelerCombatUnit> SelectAliveTravelers(BattleState battleState)
        => battleState.TravelerTeam.Where(traveler => traveler.IsAlive);
}

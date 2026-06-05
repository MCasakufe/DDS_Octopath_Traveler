namespace Octopath_Traveler_Models.Battle;

internal sealed class HighestSpeedBeastTravelerSkillTargetSelector : OrderedBeastTravelerSkillTargetSelector
{
    protected override IOrderedEnumerable<BeastCombatUnit> OrderTargets(IReadOnlyList<BeastCombatUnit> aliveBeasts)
        => aliveBeasts
            .OrderByDescending(beast => beast.Speed)
            .ThenBy(beast => beast.BoardSlotIndex);
}

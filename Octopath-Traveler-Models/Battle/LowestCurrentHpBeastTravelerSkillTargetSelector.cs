namespace Octopath_Traveler_Models.Battle;

internal sealed class LowestCurrentHpBeastTravelerSkillTargetSelector : OrderedBeastTravelerSkillTargetSelector
{
    protected override IOrderedEnumerable<BeastCombatUnit> OrderTargets(IReadOnlyList<BeastCombatUnit> aliveBeasts)
        => aliveBeasts
            .OrderBy(beast => beast.CurrentHp)
            .ThenBy(beast => beast.BoardSlotIndex);
}

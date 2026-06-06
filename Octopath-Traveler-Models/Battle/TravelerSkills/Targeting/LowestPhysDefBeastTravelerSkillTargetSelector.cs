namespace Octopath_Traveler_Models.Battle;

internal sealed class LowestPhysDefBeastTravelerSkillTargetSelector : OrderedBeastTravelerSkillTargetSelector
{
    protected override IOrderedEnumerable<BeastCombatUnit> OrderTargets(IReadOnlyList<BeastCombatUnit> aliveBeasts)
        => aliveBeasts
            .OrderBy(beast => beast.PhysDef)
            .ThenBy(beast => beast.BoardSlotIndex);
}

namespace Octopath_Traveler_Models.Battle;

internal sealed class HighestElemAtkBeastSingleTargetSelector : OrderedSkillNameSetBeastSingleTargetSelector
{
    private static readonly HashSet<string> TargetSkills = new(StringComparer.Ordinal)
    {
        "Befuddling claw"
    };

    public HighestElemAtkBeastSingleTargetSelector()
        : base(TargetSkills)
    {
    }

    protected override IOrderedEnumerable<TravelerCombatUnit> OrderTargets(
        IReadOnlyList<TravelerCombatUnit> aliveTravelers)
        => aliveTravelers
            .OrderByDescending(traveler => traveler.ElemAtk)
            .ThenBy(traveler => traveler.BoardSlotIndex);
}

namespace Octopath_Traveler_Models.Battle;

internal sealed class HighestPhysDefBeastSingleTargetSelector : OrderedSkillNameSetBeastSingleTargetSelector
{
    private static readonly HashSet<string> TargetSkills = new(StringComparer.Ordinal)
    {
        "Consume Armor"
    };

    public HighestPhysDefBeastSingleTargetSelector()
        : base(TargetSkills)
    {
    }

    protected override IOrderedEnumerable<TravelerCombatUnit> OrderTargets(
        IReadOnlyList<TravelerCombatUnit> aliveTravelers)
        => aliveTravelers
            .OrderByDescending(traveler => traveler.PhysDef)
            .ThenBy(traveler => traveler.BoardSlotIndex);
}

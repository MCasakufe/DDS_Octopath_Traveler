namespace Octopath_Traveler_Models.Battle;

internal sealed class LowestPhysDefBeastSingleTargetSelector : OrderedSkillNameSetBeastSingleTargetSelector
{
    private static readonly HashSet<string> TargetSkills = new(StringComparer.Ordinal)
    {
        "Stab",
        "Boar Rush",
        "Vorpal Fang",
        "Double Bite",
        "Gather Strength"
    };

    public LowestPhysDefBeastSingleTargetSelector()
        : base(TargetSkills)
    {
    }

    protected override IOrderedEnumerable<TravelerCombatUnit> OrderTargets(
        IReadOnlyList<TravelerCombatUnit> aliveTravelers)
        => aliveTravelers
            .OrderBy(traveler => traveler.PhysDef)
            .ThenBy(traveler => traveler.BoardSlotIndex);
}

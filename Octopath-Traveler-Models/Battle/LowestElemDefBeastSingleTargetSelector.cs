namespace Octopath_Traveler_Models.Battle;

internal sealed class LowestElemDefBeastSingleTargetSelector : OrderedSkillNameSetBeastSingleTargetSelector
{
    private static readonly HashSet<string> TargetSkills = new(StringComparer.Ordinal)
    {
        "Windshot",
        "Firesand",
        "Thundershot",
        "Lightshot",
        "Iceshot",
        "Shadowshot"
    };

    public LowestElemDefBeastSingleTargetSelector()
        : base(TargetSkills)
    {
    }

    protected override IOrderedEnumerable<TravelerCombatUnit> OrderTargets(
        IReadOnlyList<TravelerCombatUnit> aliveTravelers)
        => aliveTravelers
            .OrderBy(traveler => traveler.ElemDef)
            .ThenBy(traveler => traveler.BoardSlotIndex);
}

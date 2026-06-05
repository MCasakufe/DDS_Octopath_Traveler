namespace Octopath_Traveler_Models.Battle;

internal sealed class HighestSpeedBeastSingleTargetSelector : OrderedSkillNameSetBeastSingleTargetSelector
{
    private static readonly HashSet<string> TargetSkills = new(StringComparer.Ordinal)
    {
        "Meteor Storm",
        "Freeze",
        "Luminescence",
        "Enshadow",
        "Wind slash"
    };

    public HighestSpeedBeastSingleTargetSelector()
        : base(TargetSkills)
    {
    }

    protected override IOrderedEnumerable<TravelerCombatUnit> OrderTargets(
        IReadOnlyList<TravelerCombatUnit> aliveTravelers)
        => aliveTravelers
            .OrderByDescending(traveler => traveler.Speed)
            .ThenBy(traveler => traveler.BoardSlotIndex);
}

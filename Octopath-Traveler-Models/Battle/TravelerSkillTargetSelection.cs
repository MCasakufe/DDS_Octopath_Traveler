namespace Octopath_Traveler_Models.Battle;

internal sealed record TravelerSkillTargetSelection(
    IReadOnlyList<BeastCombatUnit> BeastTargets,
    IReadOnlyList<TravelerCombatUnit> TravelerTargets)
{
    public static TravelerSkillTargetSelection Empty { get; } = new([], []);

    public BeastCombatUnit? SingleBeastTarget => BeastTargets.Count == 0 ? null : BeastTargets[0];

    public TravelerCombatUnit? SingleTravelerTarget => TravelerTargets.Count == 0 ? null : TravelerTargets[0];

    public static TravelerSkillTargetSelection WithBeast(BeastCombatUnit beastTarget)
        => new([beastTarget], []);

    public static TravelerSkillTargetSelection WithBeasts(IReadOnlyList<BeastCombatUnit> beastTargets)
        => new(beastTargets, []);

    public static TravelerSkillTargetSelection WithTraveler(TravelerCombatUnit travelerTarget)
        => new([], [travelerTarget]);

    public static TravelerSkillTargetSelection WithTravelers(IReadOnlyList<TravelerCombatUnit> travelerTargets)
        => new([], travelerTargets);
}

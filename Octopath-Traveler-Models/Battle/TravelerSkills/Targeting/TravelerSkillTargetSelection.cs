namespace Octopath_Traveler_Models.Battle;

internal sealed record TravelerSkillTargetSelection(
    IReadOnlyList<BeastCombatUnit> BeastTargets,
    IReadOnlyList<TravelerCombatUnit> TravelerTargets)
{
    private const int NoTargets = 0;
    private const int FirstTargetIndex = 0;

    public static TravelerSkillTargetSelection Empty { get; } = new([], []);

    public BeastCombatUnit? SingleBeastTarget
        => BeastTargets.Count == NoTargets ? null : BeastTargets[FirstTargetIndex];

    public TravelerCombatUnit? SingleTravelerTarget
        => TravelerTargets.Count == NoTargets ? null : TravelerTargets[FirstTargetIndex];

    public static TravelerSkillTargetSelection WithBeast(BeastCombatUnit beastTarget)
        => new([beastTarget], []);

    public static TravelerSkillTargetSelection WithBeasts(IReadOnlyList<BeastCombatUnit> beastTargets)
        => new(beastTargets, []);

    public static TravelerSkillTargetSelection WithTraveler(TravelerCombatUnit travelerTarget)
        => new([], [travelerTarget]);

    public static TravelerSkillTargetSelection WithTravelers(IReadOnlyList<TravelerCombatUnit> travelerTargets)
        => new([], travelerTargets);
}

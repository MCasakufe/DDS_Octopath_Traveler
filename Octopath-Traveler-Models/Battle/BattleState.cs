namespace Octopath_Traveler_Models.Battle;

public sealed class BattleState
{
    public BattleState(int roundNumber, IReadOnlyList<TravelerCombatUnit> travelerTeam, IReadOnlyList<BeastCombatUnit> beastTeam)
    {
        RoundNumber = roundNumber;
        TravelerTeam = travelerTeam;
        BeastTeam = beastTeam;
    }

    public int RoundNumber { get; set; }

    public IReadOnlyList<TravelerCombatUnit> TravelerTeam { get; }

    public IReadOnlyList<BeastCombatUnit> BeastTeam { get; }
}

namespace Octopath_Traveler_Models.Battle;

public sealed class BattleState
{
    public BattleState(int roundNumber, IReadOnlyList<TravelerCombatUnit> travelerTeam, IReadOnlyList<BeastCombatUnit> beastTeam)
        : this(
            roundNumber,
            travelerTeam,
            beastTeam,
            new PassiveSkillNotifierFactory().Create(travelerTeam))
    {
    }

    public BattleState(
        int roundNumber,
        IReadOnlyList<TravelerCombatUnit> travelerTeam,
        IReadOnlyList<BeastCombatUnit> beastTeam,
        PassiveSkillNotifier passiveSkillNotifier)
    {
        RoundNumber = roundNumber;
        TravelerTeam = travelerTeam;
        BeastTeam = beastTeam;
        PassiveSkillNotifier = passiveSkillNotifier;
    }

    public int RoundNumber { get; set; }

    public IReadOnlyList<TravelerCombatUnit> TravelerTeam { get; }

    public IReadOnlyList<BeastCombatUnit> BeastTeam { get; }

    public PassiveSkillNotifier PassiveSkillNotifier { get; }
}

using Octopath_Traveler_Models.Battle;

namespace Octopath_Traveler.Battle;

internal sealed class RoundExecutionState
{
    private readonly HashSet<TurnParticipantKey> _actedParticipants = [];
    private readonly HashSet<int> _travelersWithGrantedPatienceTurn = [];
    private readonly Queue<int> _pendingPatienceTravelerTurns = new();

    public IReadOnlySet<TurnParticipantKey> ActedParticipants => _actedParticipants;

    public void MarkParticipantAsActed(TurnParticipant participant)
        => _actedParticipants.Add(new TurnParticipantKey(participant.Side, participant.BoardSlotIndex));

    public bool HasPendingPatienceTurns()
        => _pendingPatienceTravelerTurns.Count > 0;

    public bool HasNoPendingPatienceTurns()
        => _pendingPatienceTravelerTurns.Count == 0;

    public int DequeuePendingPatienceTurn()
        => _pendingPatienceTravelerTurns.Dequeue();

    public void GrantPatienceTurn(TravelerCombatUnit traveler)
    {
        _travelersWithGrantedPatienceTurn.Add(traveler.BoardSlotIndex);
        _pendingPatienceTravelerTurns.Enqueue(traveler.BoardSlotIndex);
    }

    public IEnumerable<TravelerCombatUnit> SelectPendingPatienceTravelers(BattleState battleState)
        => _pendingPatienceTravelerTurns.Select(boardSlotIndex => battleState.TravelerTeam[boardSlotIndex]);

    public IReadOnlyList<TravelerCombatUnit> SelectExtraTurnEligibleTravelers(BattleState battleState)
    {
        PassiveSkillNotifier passiveSkillNotifier = battleState.PassiveSkillNotifier;
        return passiveSkillNotifier.SelectExtraTurnEligibleTravelers(_travelersWithGrantedPatienceTurn);
    }
}

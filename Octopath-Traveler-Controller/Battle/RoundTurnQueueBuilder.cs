namespace Octopath_Traveler.Battle;

public enum BattleSide
{
    Traveler,
    Beast
}

public readonly record struct TurnParticipantKey(BattleSide Side, int BoardSlotIndex);

public sealed record TurnParticipant(string Name, int Speed, BattleSide Side, int BoardSlotIndex);

public sealed record RoundTurnQueues(IReadOnlyList<TurnParticipant> CurrentRound, IReadOnlyList<TurnParticipant> NextRound);

public sealed class RoundTurnQueueBuilder
{
    public RoundTurnQueues CreateQueues(BattleState battleState, IReadOnlySet<TurnParticipantKey> actedParticipants)
    {
        var nextRoundQueue = BuildAliveTurnOrder(battleState);
        var currentRoundQueue = nextRoundQueue
            .Where(participant => !actedParticipants.Contains(new TurnParticipantKey(participant.Side, participant.BoardSlotIndex)))
            .ToList();

        return new RoundTurnQueues(currentRoundQueue, nextRoundQueue);
    }

    private static List<TurnParticipant> BuildAliveTurnOrder(BattleState battleState)
    {
        var aliveParticipants = GetAliveTravelers(battleState)
            .Concat(GetAliveBeasts(battleState));

        return aliveParticipants
            .OrderByDescending(participant => participant.Speed)
            .ThenBy(GetSidePriority)
            .ThenBy(participant => participant.BoardSlotIndex)
            .ToList();
    }

    private static IEnumerable<TurnParticipant> GetAliveTravelers(BattleState battleState)
        => battleState.TravelerTeam
            .Where(traveler => traveler.IsAlive)
            .Select(traveler => new TurnParticipant(traveler.Name, traveler.Speed, BattleSide.Traveler, traveler.BoardSlotIndex));

    private static IEnumerable<TurnParticipant> GetAliveBeasts(BattleState battleState)
        => battleState.BeastTeam
            .Where(beast => beast.IsAlive)
            .Select(beast => new TurnParticipant(beast.Name, beast.Speed, BattleSide.Beast, beast.BoardSlotIndex));

    private static int GetSidePriority(TurnParticipant participant)
        => participant.Side == BattleSide.Traveler ? 0 : 1;
}
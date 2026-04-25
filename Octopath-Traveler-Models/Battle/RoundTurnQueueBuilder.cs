namespace Octopath_Traveler_Models.Battle;

public enum BattleSide
{
    Traveler,
    Beast
}

public readonly record struct TurnParticipantKey(BattleSide Side, int BoardSlotIndex);

public sealed record TurnParticipant(
    string Name,
    int Speed,
    BattleSide Side,
    int BoardSlotIndex,
    bool HasRecoveryPriority,
    bool HasDefendPriority,
    bool HasIncreasedPriority,
    bool HasDecreasedPriority);

public sealed record RoundTurnQueues(IReadOnlyList<TurnParticipant> CurrentRound, IReadOnlyList<TurnParticipant> NextRound);

public sealed class RoundTurnQueueBuilder
{
    private const int ZeroRoundsRemaining = 0;
    private const int OneRoundRemaining = 1;
    private const int PriorityBucketRecovery = 0;
    private const int PriorityBucketDefend = 1;
    private const int PriorityBucketIncreased = 2;
    private const int PriorityBucketNormal = 3;
    private const int PriorityBucketDecreased = 4;

    public RoundTurnQueues BuildRoundTurnQueues(BattleState battleState, IReadOnlySet<TurnParticipantKey> actedParticipants)
    {
        List<TurnParticipant> currentRoundOrder = BuildAliveTurnOrder(
            battleState,
            TurnQueueProjection.CurrentRound);
        List<TurnParticipant> nextRoundQueue = BuildAliveTurnOrder(
            battleState,
            TurnQueueProjection.NextRound);
        List<TurnParticipant> currentRoundQueue = currentRoundOrder
            .Where(participant => !actedParticipants.Contains(new TurnParticipantKey(participant.Side, participant.BoardSlotIndex)))
            .ToList();

        return new RoundTurnQueues(currentRoundQueue, nextRoundQueue);
    }

    private static List<TurnParticipant> BuildAliveTurnOrder(
        BattleState battleState,
        TurnQueueProjection turnQueueProjection)
    {
        IEnumerable<TurnParticipant> aliveParticipants = GetAliveTravelers(
                battleState,
                turnQueueProjection)
            .Concat(GetAliveBeasts(battleState, turnQueueProjection));

        IOrderedEnumerable<TurnParticipant> orderedParticipants = aliveParticipants
            .OrderBy(GetPriorityBucket)
            .ThenByDescending(participant => participant.Speed)
            .ThenBy(GetSidePriority)
            .ThenBy(participant => participant.BoardSlotIndex);

        return orderedParticipants.ToList();
    }

    private static IEnumerable<TurnParticipant> GetAliveTravelers(
        BattleState battleState,
        TurnQueueProjection turnQueueProjection)
        => battleState.TravelerTeam
            .Where(traveler => traveler.IsAlive)
            .Where(traveler => IsTravelerReadyToAct(traveler, turnQueueProjection))
            .Select(traveler => new TurnParticipant(
                traveler.Name,
                traveler.Speed,
                BattleSide.Traveler,
                traveler.BoardSlotIndex,
                HasRecoveryPriority: false,
                ResolveTravelerDefendPriority(traveler, turnQueueProjection),
                ResolveTravelerIncreasedPriority(traveler, turnQueueProjection),
                HasDecreasedPriority: false));

    private static IEnumerable<TurnParticipant> GetAliveBeasts(
        BattleState battleState,
        TurnQueueProjection turnQueueProjection)
        => battleState.BeastTeam
            .Where(beast => beast.IsAlive)
            .Where(beast => IsReadyToAct(beast, turnQueueProjection))
            .Select(beast => new TurnParticipant(
                beast.Name,
                beast.Speed,
                BattleSide.Beast,
                beast.BoardSlotIndex,
                HasRecoveryPriority: ResolveRecoveryPriority(beast, turnQueueProjection),
                HasDefendPriority: false,
                HasIncreasedPriority: false,
                HasDecreasedPriority: turnQueueProjection == TurnQueueProjection.NextRound
                    ? beast.RemainingDecreasedPriorityRounds > OneRoundRemaining
                    : beast.RemainingDecreasedPriorityRounds > ZeroRoundsRemaining));

    private static bool IsTravelerReadyToAct(
        TravelerCombatUnit traveler,
        TurnQueueProjection turnQueueProjection)
        => turnQueueProjection == TurnQueueProjection.NextRound || !traveler.IsWaitingForNextRoundAfterRevive;

    private static bool ResolveTravelerDefendPriority(
        TravelerCombatUnit traveler,
        TurnQueueProjection turnQueueProjection)
        => turnQueueProjection == TurnQueueProjection.NextRound
            ? traveler.HasPendingDefendPriority
            : traveler.HasDefendPriorityCurrentRound;

    private static bool ResolveTravelerIncreasedPriority(
        TravelerCombatUnit traveler,
        TurnQueueProjection turnQueueProjection)
        => turnQueueProjection == TurnQueueProjection.NextRound
            ? traveler.HasPendingIncreasedPriority
            : traveler.HasIncreasedPriorityCurrentRound;

    private static bool IsReadyToAct(
        BeastCombatUnit beast,
        TurnQueueProjection turnQueueProjection)
    {
        if (turnQueueProjection == TurnQueueProjection.CurrentRound)
            return beast.RemainingBreakingRounds == ZeroRoundsRemaining;

        return beast.RemainingBreakingRounds <= OneRoundRemaining;
    }

    private static bool ResolveRecoveryPriority(
        BeastCombatUnit beast,
        TurnQueueProjection turnQueueProjection)
    {
        if (turnQueueProjection == TurnQueueProjection.CurrentRound)
            return beast.HasRecoveryPriorityCurrentRound;

        return beast.RemainingBreakingRounds == OneRoundRemaining;
    }

    private static int GetPriorityBucket(TurnParticipant participant)
    {
        if (participant.HasRecoveryPriority)
            return PriorityBucketRecovery;

        if (participant.HasDefendPriority)
            return PriorityBucketDefend;

        if (participant.HasIncreasedPriority)
            return PriorityBucketIncreased;

        if (participant.HasDecreasedPriority)
            return PriorityBucketDecreased;

        return PriorityBucketNormal;
    }

    private static int GetSidePriority(TurnParticipant participant)
        => participant.Side == BattleSide.Traveler ? 0 : 1;

    private enum TurnQueueProjection
    {
        CurrentRound,
        NextRound
    }
}

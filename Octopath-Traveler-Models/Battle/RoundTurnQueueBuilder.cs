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
    public RoundTurnQueues CreateQueues(BattleState battleState, IReadOnlySet<TurnParticipantKey> actedParticipants)
    {
        List<TurnParticipant> currentRoundOrder = BuildAliveTurnOrder(
            battleState,
            usePendingTravelerPriorities: false,
            includeRevivedTravelers: false,
            useProjectedBeastDecreasedPriority: false);
        List<TurnParticipant> nextRoundQueue = BuildAliveTurnOrder(
            battleState,
            usePendingTravelerPriorities: true,
            includeRevivedTravelers: true,
            useProjectedBeastDecreasedPriority: true);
        List<TurnParticipant> currentRoundQueue = currentRoundOrder
            .Where(participant => !actedParticipants.Contains(new TurnParticipantKey(participant.Side, participant.BoardSlotIndex)))
            .ToList();

        return new RoundTurnQueues(currentRoundQueue, nextRoundQueue);
    }

    private static List<TurnParticipant> BuildAliveTurnOrder(
        BattleState battleState,
        bool usePendingTravelerPriorities,
        bool includeRevivedTravelers,
        bool useProjectedBeastDecreasedPriority)
    {
        IEnumerable<TurnParticipant> aliveParticipants = GetAliveTravelers(
                battleState,
                usePendingTravelerPriorities,
                includeRevivedTravelers)
            .Concat(GetAliveBeasts(battleState, useProjectedBeastDecreasedPriority));

        IOrderedEnumerable<TurnParticipant> orderedParticipants = aliveParticipants
            .OrderBy(GetPriorityBucket)
            .ThenByDescending(participant => participant.Speed)
            .ThenBy(GetSidePriority)
            .ThenBy(participant => participant.BoardSlotIndex);

        return orderedParticipants.ToList();
    }

    private static IEnumerable<TurnParticipant> GetAliveTravelers(
        BattleState battleState,
        bool usePendingTravelerPriorities,
        bool includeRevivedTravelers)
        => battleState.TravelerTeam
            .Where(traveler => traveler.IsAlive)
            .Where(traveler => includeRevivedTravelers || !traveler.IsWaitingForNextRoundAfterRevive)
            .Select(traveler => new TurnParticipant(
                traveler.Name,
                traveler.Speed,
                BattleSide.Traveler,
                traveler.BoardSlotIndex,
                HasRecoveryPriority: false,
                usePendingTravelerPriorities ? traveler.HasPendingDefendPriority : traveler.HasDefendPriorityCurrentRound,
                usePendingTravelerPriorities ? traveler.HasPendingIncreasedPriority : traveler.HasIncreasedPriorityCurrentRound,
                HasDecreasedPriority: false));

    private static IEnumerable<TurnParticipant> GetAliveBeasts(BattleState battleState, bool useProjectedDecreasedPriority)
        => battleState.BeastTeam
            .Where(beast => beast.IsAlive)
            .Where(beast => IsReadyToAct(beast, useProjectedDecreasedPriority))
            .Select(beast => new TurnParticipant(
                beast.Name,
                beast.Speed,
                BattleSide.Beast,
                beast.BoardSlotIndex,
                HasRecoveryPriority: ResolveRecoveryPriority(beast, useProjectedDecreasedPriority),
                HasDefendPriority: false,
                HasIncreasedPriority: false,
                HasDecreasedPriority: useProjectedDecreasedPriority
                    ? beast.RemainingDecreasedPriorityRounds > 1
                    : beast.RemainingDecreasedPriorityRounds > 0));

    private static bool IsReadyToAct(BeastCombatUnit beast, bool useProjectedDecreasedPriority)
    {
        if (!useProjectedDecreasedPriority)
            return beast.RemainingBreakingRounds == 0;

        return beast.RemainingBreakingRounds <= 1;
    }

    private static bool ResolveRecoveryPriority(BeastCombatUnit beast, bool useProjectedDecreasedPriority)
    {
        if (!useProjectedDecreasedPriority)
            return beast.HasRecoveryPriorityCurrentRound;

        return beast.RemainingBreakingRounds == 1;
    }

    private static int GetPriorityBucket(TurnParticipant participant)
    {
        if (participant.HasRecoveryPriority)
            return 0;

        if (participant.HasDefendPriority)
            return 1;

        if (participant.HasIncreasedPriority)
            return 2;

        if (participant.HasDecreasedPriority)
            return 4;

        return 3;
    }

    private static int GetSidePriority(TurnParticipant participant)
        => participant.Side == BattleSide.Traveler ? 0 : 1;
}

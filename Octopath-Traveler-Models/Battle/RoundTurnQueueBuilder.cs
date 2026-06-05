namespace Octopath_Traveler_Models.Battle;

public sealed class RoundTurnQueueBuilder
{
    private const int ZeroRoundsRemaining = 0;
    private const int OneRoundRemaining = 1;
    private const int PriorityBucketRecovery = 0;
    private const int PriorityBucketDefend = 1;
    private const int PriorityBucketIncreased = 2;
    private const int PriorityBucketNormal = 3;
    private const int PriorityBucketDecreased = 4;
    private const int TravelerSidePriority = 0;
    private const int BeastSidePriority = 1;

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
            .Select(traveler => BuildTravelerParticipant(traveler, turnQueueProjection));

    private static IEnumerable<TurnParticipant> GetAliveBeasts(
        BattleState battleState,
        TurnQueueProjection turnQueueProjection)
        => battleState.BeastTeam
            .Where(beast => beast.IsAlive)
            .Where(beast => IsReadyToAct(beast, turnQueueProjection))
            .Select(beast => BuildBeastParticipant(beast, turnQueueProjection));

    private static TurnParticipant BuildTravelerParticipant(
        TravelerCombatUnit traveler,
        TurnQueueProjection turnQueueProjection)
        => new(
            traveler.Name,
            SelectProjectedSpeed(traveler, turnQueueProjection),
            BattleSide.Traveler,
            traveler.BoardSlotIndex,
            HasRecoveryPriority: false,
            DetermineTravelerDefendPriority(traveler, turnQueueProjection),
            DetermineTravelerIncreasedPriority(traveler, turnQueueProjection),
            HasDecreasedPriority: false);

    private static TurnParticipant BuildBeastParticipant(
        BeastCombatUnit beast,
        TurnQueueProjection turnQueueProjection)
        => new(
            beast.Name,
            SelectProjectedSpeed(beast, turnQueueProjection),
            BattleSide.Beast,
            beast.BoardSlotIndex,
            HasRecoveryPriority: DetermineBeastRecoveryPriority(beast, turnQueueProjection),
            HasDefendPriority: false,
            HasIncreasedPriority: false,
            HasDecreasedPriority: HasDecreasedPriority(beast, turnQueueProjection));

    private static bool HasDecreasedPriority(BeastCombatUnit beast, TurnQueueProjection turnQueueProjection)
        => turnQueueProjection == TurnQueueProjection.NextRound
            ? beast.RemainingDecreasedPriorityRounds > OneRoundRemaining
            : beast.RemainingDecreasedPriorityRounds > ZeroRoundsRemaining;

    private static int SelectProjectedSpeed(Unit unit, TurnQueueProjection turnQueueProjection)
        => turnQueueProjection == TurnQueueProjection.NextRound
            ? unit.GetEffectiveSpeedAfterRoundCountdown()
            : unit.GetEffectiveSpeed();

    private static bool IsTravelerReadyToAct(
        TravelerCombatUnit traveler,
        TurnQueueProjection turnQueueProjection)
        => turnQueueProjection == TurnQueueProjection.NextRound || !traveler.IsWaitingForNextRoundAfterRevive;

    private static bool DetermineTravelerDefendPriority(
        TravelerCombatUnit traveler,
        TurnQueueProjection turnQueueProjection)
        => turnQueueProjection == TurnQueueProjection.NextRound
            ? traveler.HasPendingDefendPriority
            : traveler.HasDefendPriorityCurrentRound;

    private static bool DetermineTravelerIncreasedPriority(
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

    private static bool DetermineBeastRecoveryPriority(
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
        => participant.Side == BattleSide.Traveler ? TravelerSidePriority : BeastSidePriority;

}

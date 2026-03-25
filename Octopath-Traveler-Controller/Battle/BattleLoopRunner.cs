namespace Octopath_Traveler.Battle;

public sealed class BattleLoopRunner
{
    private const int MaxTravelerBp = 5;

    private readonly RoundTurnQueueBuilder _roundTurnQueueBuilder;
    private readonly RoundStateRenderer _roundStateRenderer;
    private readonly TravelerTurnFlow _travelerTurnFlow;
    private readonly TravelerBasicAttackResolver _travelerBasicAttackResolver;
    private readonly BeastAttackResolver _beastAttackResolver;
    private readonly BattleVictoryResolver _battleVictoryResolver;

    public BattleLoopRunner(
        RoundTurnQueueBuilder roundTurnQueueBuilder,
        RoundStateRenderer roundStateRenderer,
        TravelerTurnFlow travelerTurnFlow,
        TravelerBasicAttackResolver travelerBasicAttackResolver,
        BeastAttackResolver beastAttackResolver,
        BattleVictoryResolver battleVictoryResolver)
    {
        _roundTurnQueueBuilder = roundTurnQueueBuilder;
        _roundStateRenderer = roundStateRenderer;
        _travelerTurnFlow = travelerTurnFlow;
        _travelerBasicAttackResolver = travelerBasicAttackResolver;
        _beastAttackResolver = beastAttackResolver;
        _battleVictoryResolver = battleVictoryResolver;
    }

    public void Run(BattleState battleState)
    {
        while (TryRunRound(battleState))
            StartNextRound(battleState);
    }

    private bool TryRunRound(BattleState battleState)
    {
        var actedParticipants = new HashSet<TurnParticipantKey>();
        RenderRoundStart(battleState, actedParticipants);

        while (TryGetNextRoundParticipant(battleState, actedParticipants, out var participant))
        {
            if (!TryResolveTurn(participant, battleState))
                return false;

            actedParticipants.Add(new TurnParticipantKey(participant.Side, participant.BoardSlotIndex));

            if (TryWriteWinnerIfBattleEnded(battleState))
                return false;

            RenderBattleSnapshotIfRoundContinues(battleState, actedParticipants);
        }

        return true;
    }

    private void RenderRoundStart(BattleState battleState, IReadOnlySet<TurnParticipantKey> actedParticipants)
    {
        var initialQueues = _roundTurnQueueBuilder.CreateQueues(battleState, actedParticipants);
        _roundStateRenderer.RenderRoundState(battleState, initialQueues);
    }

    private bool TryGetNextRoundParticipant(
        BattleState battleState,
        IReadOnlySet<TurnParticipantKey> actedParticipants,
        out TurnParticipant participant)
    {
        var queues = _roundTurnQueueBuilder.CreateQueues(battleState, actedParticipants);
        if (queues.CurrentRound.Count == 0)
        {
            participant = null!;
            return false;
        }

        participant = queues.CurrentRound[0];
        return true;
    }

    private bool TryResolveTurn(TurnParticipant participant, BattleState battleState)
        => participant.Side switch
        {
            BattleSide.Traveler => TryResolveTravelerTurn(participant, battleState),
            BattleSide.Beast => ResolveBeastTurn(participant, battleState),
            _ => false
        };

    private bool TryResolveTravelerTurn(TurnParticipant participant, BattleState battleState)
    {
        var traveler = battleState.TravelerTeam[participant.BoardSlotIndex];
        var turnOutcome = _travelerTurnFlow.RunTurn(traveler, battleState);

        if (turnOutcome.Resolution == TravelerTurnResolution.Fled)
        {
            _battleVictoryResolver.WriteEnemyWinnerAfterFlee();
            return false;
        }

        if (turnOutcome.Resolution == TravelerTurnResolution.BasicAttackChosen
            && turnOutcome.SelectedWeapon is not null
            && turnOutcome.SelectedTarget is not null)
        {
            _travelerBasicAttackResolver.Resolve(traveler, turnOutcome.SelectedTarget, turnOutcome.SelectedWeapon);
        }

        return true;
    }

    private bool TryWriteWinnerIfBattleEnded(BattleState battleState)
    {
        var winner = _battleVictoryResolver.Evaluate(battleState);
        if (winner == BattleWinner.None)
            return false;

        _battleVictoryResolver.WriteWinner(winner);
        return true;
    }

    private void RenderBattleSnapshotIfRoundContinues(
        BattleState battleState,
        IReadOnlySet<TurnParticipantKey> actedParticipants)
    {
        var updatedQueues = _roundTurnQueueBuilder.CreateQueues(battleState, actedParticipants);
        if (updatedQueues.CurrentRound.Count > 0)
            _roundStateRenderer.RenderBattleSnapshot(battleState, updatedQueues);
    }

    private bool ResolveBeastTurn(TurnParticipant participant, BattleState battleState)
    {
        var beast = battleState.BeastTeam[participant.BoardSlotIndex];
        _beastAttackResolver.Resolve(beast, battleState);
        return true;
    }

    private static void StartNextRound(BattleState battleState)
    {
        IncreaseAliveTravelerBp(battleState);
        battleState.RoundNumber += 1;
    }

    private static void IncreaseAliveTravelerBp(BattleState battleState)
    {
        foreach (var traveler in battleState.TravelerTeam.Where(traveler => traveler.IsAlive))
            traveler.CurrentBp = Math.Min(MaxTravelerBp, traveler.CurrentBp + 1);
    }
}

namespace Octopath_Traveler.Battle;

public sealed class BattleLoopRunner
{
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
        while (true)
        {
            var actedParticipants = new HashSet<TurnParticipantKey>();
            var initialQueues = _roundTurnQueueBuilder.CreateQueues(battleState, actedParticipants);
            _roundStateRenderer.RenderRoundState(battleState, initialQueues);

            var roundEnded = RunRoundTurns(battleState, actedParticipants);
            if (!roundEnded)
                return;

            IncreaseAliveTravelerBp(battleState);
            battleState.RoundNumber += 1;
        }
    }

    private bool RunRoundTurns(BattleState battleState, HashSet<TurnParticipantKey> actedParticipants)
    {
        while (true)
        {
            var queues = _roundTurnQueueBuilder.CreateQueues(battleState, actedParticipants);
            if (queues.CurrentRound.Count == 0)
                return true;

            var nextParticipant = queues.CurrentRound[0];
            if (!ResolveTurn(nextParticipant, battleState))
                return false;

            actedParticipants.Add(new TurnParticipantKey(nextParticipant.Side, nextParticipant.BoardSlotIndex));

            var winner = _battleVictoryResolver.Evaluate(battleState);
            if (winner != BattleWinner.None)
            {
                _battleVictoryResolver.WriteWinner(winner);
                return false;
            }

            var updatedQueues = _roundTurnQueueBuilder.CreateQueues(battleState, actedParticipants);
            if (updatedQueues.CurrentRound.Count > 0)
                _roundStateRenderer.RenderBattleSnapshot(battleState, updatedQueues);
        }
    }

    private bool ResolveTurn(TurnParticipant participant, BattleState battleState)
    {
        if (participant.Side == BattleSide.Traveler)
            return ResolveTravelerTurn(participant, battleState);

        return ResolveBeastTurn(participant, battleState);
    }

    private bool ResolveTravelerTurn(TurnParticipant participant, BattleState battleState)
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

    private bool ResolveBeastTurn(TurnParticipant participant, BattleState battleState)
    {
        var beast = battleState.BeastTeam[participant.BoardSlotIndex];
        _beastAttackResolver.Resolve(beast, battleState);
        return true;
    }

    private static void IncreaseAliveTravelerBp(BattleState battleState)
    {
        foreach (var traveler in battleState.TravelerTeam.Where(traveler => traveler.IsAlive))
            traveler.CurrentBp = Math.Min(5, traveler.CurrentBp + 1);
    }
}
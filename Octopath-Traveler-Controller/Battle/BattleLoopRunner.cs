namespace Octopath_Traveler.Battle;

public sealed class BattleLoopRunner
{
    private const int MaxTravelerBp = 5;

    private readonly RoundTurnQueueBuilder _roundTurnQueueBuilder;
    private readonly BattleStatePrinter _battleStatePrinter;
    private readonly TravelerTurnFlow _travelerTurnFlow;
    private readonly TravelerBasicAttackExecutor _travelerBasicAttackExecutor;
    private readonly BeastAttackExecutor _beastAttackExecutor;
    private readonly BattleActionPrinter _battleActionPrinter;
    private readonly BattleWinnerService _battleWinnerService;

    public BattleLoopRunner(
        RoundTurnQueueBuilder roundTurnQueueBuilder,
        BattleStatePrinter battleStatePrinter,
        TravelerTurnFlow travelerTurnFlow,
        TravelerBasicAttackExecutor travelerBasicAttackExecutor,
        BeastAttackExecutor beastAttackExecutor,
        BattleActionPrinter battleActionPrinter,
        BattleWinnerService battleWinnerService)
    {
        _roundTurnQueueBuilder = roundTurnQueueBuilder;
        _battleStatePrinter = battleStatePrinter;
        _travelerTurnFlow = travelerTurnFlow;
        _travelerBasicAttackExecutor = travelerBasicAttackExecutor;
        _beastAttackExecutor = beastAttackExecutor;
        _battleActionPrinter = battleActionPrinter;
        _battleWinnerService = battleWinnerService;
    }

    public void Run(BattleState battleState)
    {
        while (ExecuteRound(battleState))
            StartNextRound(battleState);
    }

    private bool ExecuteRound(BattleState battleState)
    {
        var actedParticipants = new HashSet<TurnParticipantKey>();
        PrintRoundStart(battleState, actedParticipants);

        var participant = GetNextRoundParticipant(battleState, actedParticipants);
        while (participant is not null)
        {
            if (ExecuteTurn(participant, battleState) == TurnExecutionResult.EndBattle)
                return false;

            MarkParticipantAsActed(participant, actedParticipants);

            var winner = GetBattleWinner(battleState);
            if (winner is not null)
            {
                _battleWinnerService.WriteWinner(winner.Value);
                return false;
            }

            PrintBattleSnapshotIfRoundContinues(battleState, actedParticipants);
            participant = GetNextRoundParticipant(battleState, actedParticipants);
        }

        return true;
    }

    private static void MarkParticipantAsActed(
        TurnParticipant participant,
        ISet<TurnParticipantKey> actedParticipants)
        => actedParticipants.Add(new TurnParticipantKey(participant.Side, participant.BoardSlotIndex));

    private void PrintRoundStart(BattleState battleState, IReadOnlySet<TurnParticipantKey> actedParticipants)
    {
        var initialQueues = _roundTurnQueueBuilder.CreateQueues(battleState, actedParticipants);
        _battleStatePrinter.PrintRoundState(battleState, initialQueues);
    }

    private TurnParticipant? GetNextRoundParticipant(
        BattleState battleState,
        IReadOnlySet<TurnParticipantKey> actedParticipants)
    {
        var queues = _roundTurnQueueBuilder.CreateQueues(battleState, actedParticipants);
        return queues.CurrentRound.Count == 0 ? null : queues.CurrentRound[0];
    }

    private TurnExecutionResult ExecuteTurn(TurnParticipant participant, BattleState battleState)
        => participant.Side switch
        {
            BattleSide.Traveler => ExecuteTravelerTurn(participant, battleState),
            BattleSide.Beast => ExecuteBeastTurn(participant, battleState),
            _ => TurnExecutionResult.EndBattle
        };

    private TurnExecutionResult ExecuteTravelerTurn(TurnParticipant participant, BattleState battleState)
    {
        var traveler = battleState.TravelerTeam[participant.BoardSlotIndex];
        var turnOutcome = _travelerTurnFlow.RunTurn(traveler, battleState);

        if (turnOutcome.Resolution == TravelerTurnResolution.Fled)
            return EndBattleAfterFlee();

        ExecuteTravelerBasicAttackIfChosen(traveler, turnOutcome);
        return TurnExecutionResult.ContinueBattle;
    }

    private TurnExecutionResult EndBattleAfterFlee()
    {
        _battleWinnerService.WriteEnemyWinnerAfterFlee();
        return TurnExecutionResult.EndBattle;
    }

    private void ExecuteTravelerBasicAttackIfChosen(TravelerCombatUnit traveler, TravelerTurnOutcome turnOutcome)
    {
        if (turnOutcome.Resolution != TravelerTurnResolution.BasicAttackChosen
            || turnOutcome.SelectedWeapon is null
            || turnOutcome.SelectedTarget is null)
        {
            return;
        }

        var attack = _travelerBasicAttackExecutor.ExecuteAttack(
            traveler,
            turnOutcome.SelectedTarget,
            turnOutcome.SelectedWeapon);
        _battleActionPrinter.PrintTravelerBasicAttack(attack);
    }

    private BattleWinner? GetBattleWinner(BattleState battleState)
    {
        var winner = _battleWinnerService.GetWinner(battleState);
        return winner == BattleWinner.None ? null : winner;
    }

    private void PrintBattleSnapshotIfRoundContinues(
        BattleState battleState,
        IReadOnlySet<TurnParticipantKey> actedParticipants)
    {
        var updatedQueues = _roundTurnQueueBuilder.CreateQueues(battleState, actedParticipants);
        if (updatedQueues.CurrentRound.Count > 0)
            _battleStatePrinter.PrintBattleSnapshot(battleState, updatedQueues);
    }

    private TurnExecutionResult ExecuteBeastTurn(TurnParticipant participant, BattleState battleState)
    {
        var beast = battleState.BeastTeam[participant.BoardSlotIndex];
        var attack = _beastAttackExecutor.ExecuteAttack(beast, battleState);
        if (attack is not null)
            _battleActionPrinter.PrintBeastAttack(attack);

        return TurnExecutionResult.ContinueBattle;
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

    private enum TurnExecutionResult
    {
        ContinueBattle,
        EndBattle
    }
}

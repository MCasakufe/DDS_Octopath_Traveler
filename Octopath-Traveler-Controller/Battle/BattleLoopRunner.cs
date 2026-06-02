using Octopath_Traveler_Models.Battle;
using Octopath_Traveler_View.Battle;

namespace Octopath_Traveler.Battle;

public sealed class BattleLoopRunner
{
    private const int RoundIncrement = 1;

    private readonly RoundTurnQueueBuilder _roundTurnQueueBuilder;
    private readonly BattleConsoleView _battleConsoleView;
    private readonly TravelerBasicAttackTurnCommand _travelerBasicAttackTurnCommand;
    private readonly TravelerSkillTurnCommand _travelerSkillTurnCommand;
    private readonly TravelerDefendTurnCommand _travelerDefendTurnCommand;
    private readonly TravelerFleeTurnCommand _travelerFleeTurnCommand;
    private readonly BeastActionTurnCommand _beastActionTurnCommand;
    private readonly BattleWinnerEvaluator _battleWinnerEvaluator;

    public BattleLoopRunner(
        RoundTurnQueueBuilder roundTurnQueueBuilder,
        BattleConsoleView battleConsoleView,
        TravelerBasicAttackTurnCommand travelerBasicAttackTurnCommand,
        TravelerSkillTurnCommand travelerSkillTurnCommand,
        TravelerDefendTurnCommand travelerDefendTurnCommand,
        TravelerFleeTurnCommand travelerFleeTurnCommand,
        BeastActionTurnCommand beastActionTurnCommand,
        BattleWinnerEvaluator battleWinnerEvaluator)
    {
        _roundTurnQueueBuilder = roundTurnQueueBuilder;
        _battleConsoleView = battleConsoleView;
        _travelerBasicAttackTurnCommand = travelerBasicAttackTurnCommand;
        _travelerSkillTurnCommand = travelerSkillTurnCommand;
        _travelerDefendTurnCommand = travelerDefendTurnCommand;
        _travelerFleeTurnCommand = travelerFleeTurnCommand;
        _beastActionTurnCommand = beastActionTurnCommand;
        _battleWinnerEvaluator = battleWinnerEvaluator;
    }

    public void Run(BattleState battleState)
    {
        while (RunRound(battleState) == RoundExecutionResult.StartNextRound)
            StartNextRound(battleState);
    }

    private RoundExecutionResult RunRound(BattleState battleState)
    {
        RoundExecutionState roundState = new();
        PrintRoundStart(battleState, roundState);

        while (true)
        {
            RoundStepOutcome outcome = RunNextRoundStep(battleState, roundState);
            if (outcome == RoundStepOutcome.ContinueRound)
                continue;

            return BuildRoundExecutionResult(outcome);
        }
    }

    private RoundStepOutcome RunNextRoundStep(BattleState battleState, RoundExecutionState roundState)
    {
        TurnParticipant? participant = GetNextRoundParticipant(battleState, roundState);
        if (participant is null)
            return ContinueAfterMissingParticipant(battleState, roundState);

        TurnExecutionOutcome turnOutcome = RunTurn(participant, battleState);
        return turnOutcome == TurnExecutionOutcome.EndBattle
            ? RoundStepOutcome.EndBattle
            : ContinueAfterCompletedTurn(participant, battleState, roundState);
    }

    private static RoundExecutionResult BuildRoundExecutionResult(RoundStepOutcome outcome)
        => outcome switch
        {
            RoundStepOutcome.StartNextRound => RoundExecutionResult.StartNextRound,
            RoundStepOutcome.EndBattle => RoundExecutionResult.EndBattle,
            _ => throw new InvalidOperationException("A continuing round step cannot finish the round.")
        };

    private RoundStepOutcome ContinueAfterMissingParticipant(
        BattleState battleState,
        RoundExecutionState roundState)
    {
        IReadOnlyList<TravelerCombatUnit> eligibleTravelers =
            SelectPatienceExtraTurnEligibleTravelers(battleState, roundState);
        if (eligibleTravelers.Count == 0)
            return RoundStepOutcome.StartNextRound;

        GrantPatienceExtraTurns(eligibleTravelers, roundState);
        PrintBattleSnapshotIfRoundContinues(battleState, roundState);
        return RoundStepOutcome.ContinueRound;
    }

    private RoundStepOutcome ContinueAfterCompletedTurn(
        TurnParticipant participant,
        BattleState battleState,
        RoundExecutionState roundState)
    {
        MarkParticipantAsActed(participant, roundState);
        BattleWinner? winner = SelectBattleWinnerOrNull(battleState);
        if (winner is not null)
            return EndBattleWithWinner(winner.Value);

        PrintBattleSnapshotIfRoundContinues(battleState, roundState);
        return RoundStepOutcome.ContinueRound;
    }

    private RoundStepOutcome EndBattleWithWinner(BattleWinner winner)
    {
        _battleConsoleView.PrintWinner(winner);
        return RoundStepOutcome.EndBattle;
    }

    private static void MarkParticipantAsActed(TurnParticipant participant, RoundExecutionState roundState)
        => roundState.ActedParticipants.Add(new TurnParticipantKey(participant.Side, participant.BoardSlotIndex));

    private void PrintRoundStart(BattleState battleState, RoundExecutionState roundState)
    {
        RoundTurnQueues initialQueues = BuildRoundTurnQueues(battleState, roundState);
        _battleConsoleView.PrintRoundState(battleState, initialQueues);
    }

    private TurnParticipant? GetNextRoundParticipant(
        BattleState battleState,
        RoundExecutionState roundState)
    {
        while (roundState.PendingPatienceTravelerTurns.Count > 0)
        {
            int boardSlotIndex = roundState.PendingPatienceTravelerTurns.Dequeue();
            TravelerCombatUnit traveler = battleState.TravelerTeam[boardSlotIndex];
            if (!traveler.IsAlive)
                continue;

            return BuildTravelerTurnParticipant(traveler);
        }

        RoundTurnQueues queues = _roundTurnQueueBuilder.BuildRoundTurnQueues(battleState, roundState.ActedParticipants);
        return queues.CurrentRound.Count == 0 ? null : queues.CurrentRound[0];
    }

    private TurnExecutionOutcome RunTurn(TurnParticipant participant, BattleState battleState)
        => participant.Side switch
        {
            BattleSide.Traveler => RunTravelerTurn(participant, battleState),
            BattleSide.Beast => RunBeastTurn(participant, battleState),
            _ => TurnExecutionOutcome.EndBattle
        };

    private TurnExecutionOutcome RunTravelerTurn(TurnParticipant participant, BattleState battleState)
    {
        TravelerCombatUnit traveler = battleState.TravelerTeam[participant.BoardSlotIndex];
        TravelerTurnOutcome turnOutcome = _battleConsoleView.RequestTravelerTurn(traveler, battleState);

        if (turnOutcome.Resolution == TravelerTurnResolution.Fled)
        {
            _travelerFleeTurnCommand.Execute();
            return TurnExecutionOutcome.EndBattle;
        }

        ResolveTravelerAction(traveler, battleState, turnOutcome);
        return TurnExecutionOutcome.ContinueRound;
    }

    private void ResolveTravelerAction(TravelerCombatUnit traveler, BattleState battleState, TravelerTurnOutcome turnOutcome)
    {
        switch (turnOutcome.Resolution)
        {
            case TravelerTurnResolution.BasicAttackChosen:
                _travelerBasicAttackTurnCommand.Execute(traveler, turnOutcome);
                break;
            case TravelerTurnResolution.SkillChosen:
                _travelerSkillTurnCommand.Execute(traveler, battleState, turnOutcome);
                break;
            case TravelerTurnResolution.DefendChosen:
                _travelerDefendTurnCommand.Execute(traveler);
                break;
        }
    }

    private BattleWinner? SelectBattleWinnerOrNull(BattleState battleState)
    {
        BattleWinner winner = _battleWinnerEvaluator.EvaluateWinner(battleState);
        return winner == BattleWinner.None ? null : winner;
    }

    private void PrintBattleSnapshotIfRoundContinues(BattleState battleState, RoundExecutionState roundState)
    {
        RoundTurnQueues updatedQueues = BuildRoundTurnQueues(battleState, roundState);
        if (updatedQueues.CurrentRound.Count > 0)
            _battleConsoleView.PrintBattleSnapshot(battleState, updatedQueues);
    }

    private RoundTurnQueues BuildRoundTurnQueues(BattleState battleState, RoundExecutionState roundState)
    {
        RoundTurnQueues baseQueues =
            _roundTurnQueueBuilder.BuildRoundTurnQueues(battleState, roundState.ActedParticipants);
        if (roundState.PendingPatienceTravelerTurns.Count == 0)
            return baseQueues;

        List<TurnParticipant> pendingTravelerParticipants = roundState.PendingPatienceTravelerTurns
            .Select(boardSlotIndex => battleState.TravelerTeam[boardSlotIndex])
            .Where(traveler => traveler.IsAlive)
            .Select(BuildTravelerTurnParticipant)
            .ToList();

        List<TurnParticipant> currentQueue = pendingTravelerParticipants
            .Concat(baseQueues.CurrentRound)
            .ToList();
        return new RoundTurnQueues(currentQueue, baseQueues.NextRound);
    }

    private IReadOnlyList<TravelerCombatUnit> SelectPatienceExtraTurnEligibleTravelers(
        BattleState battleState,
        RoundExecutionState roundState)
        => battleState
            .PassiveSkillNotifier
            .SelectExtraTurnEligibleTravelers(roundState.TravelersWithGrantedPatienceTurn);

    private void GrantPatienceExtraTurns(
        IReadOnlyList<TravelerCombatUnit> eligibleTravelers,
        RoundExecutionState roundState)
    {
        foreach (TravelerCombatUnit traveler in eligibleTravelers)
        {
            roundState.TravelersWithGrantedPatienceTurn.Add(traveler.BoardSlotIndex);
            roundState.PendingPatienceTravelerTurns.Enqueue(traveler.BoardSlotIndex);
            _battleConsoleView.PrintPatienceExtraTurn(traveler.Name);
        }
    }

    private static TurnParticipant BuildTravelerTurnParticipant(TravelerCombatUnit traveler)
        => new(
            traveler.Name,
            traveler.Speed,
            BattleSide.Traveler,
            traveler.BoardSlotIndex,
            HasRecoveryPriority: false,
            traveler.HasDefendPriorityCurrentRound,
            traveler.HasIncreasedPriorityCurrentRound,
            HasDecreasedPriority: false);

    private TurnExecutionOutcome RunBeastTurn(TurnParticipant participant, BattleState battleState)
    {
        BeastCombatUnit beast = battleState.BeastTeam[participant.BoardSlotIndex];
        _beastActionTurnCommand.Execute(beast, battleState);
        return TurnExecutionOutcome.ContinueRound;
    }

    private static void StartNextRound(BattleState battleState)
    {
        ApplyTravelerRoundEndPassiveRecovery(battleState);
        PrepareTravelerRoundStates(battleState);
        PrepareBeastRoundStates(battleState);
        IncreaseAliveTravelerBp(battleState);
        battleState.RoundNumber += RoundIncrement;
    }

    private static void ApplyTravelerRoundEndPassiveRecovery(BattleState battleState)
        => battleState.PassiveSkillNotifier.NotifyRoundEnded();

    private static void PrepareTravelerRoundStates(BattleState battleState)
    {
        foreach (TravelerCombatUnit traveler in battleState.TravelerTeam)
            traveler.PrepareRoundStateForNextRound();
    }

    private static void PrepareBeastRoundStates(BattleState battleState)
    {
        foreach (BeastCombatUnit beast in battleState.BeastTeam)
            beast.PrepareRoundStateForNextRound();
    }

    private static void IncreaseAliveTravelerBp(BattleState battleState)
    {
        foreach (TravelerCombatUnit traveler in battleState.TravelerTeam)
            traveler.PrepareBpForNextRound();
    }
}

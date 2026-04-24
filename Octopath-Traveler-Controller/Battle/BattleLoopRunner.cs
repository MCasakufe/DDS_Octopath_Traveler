using Octopath_Traveler_Models.Battle;
using Octopath_Traveler_View.Battle;

namespace Octopath_Traveler.Battle;

public sealed class BattleLoopRunner
{
    private const int MaxTravelerBp = 5;

    private readonly RoundTurnQueueBuilder _roundTurnQueueBuilder;
    private readonly BattleConsoleView _battleConsoleView;
    private readonly TravelerBasicAttackExecutor _travelerBasicAttackExecutor;
    private readonly TravelerSkillExecutor _travelerSkillExecutor;
    private readonly BeastAttackExecutor _beastAttackExecutor;
    private readonly BattleWinnerEvaluator _battleWinnerEvaluator;

    public BattleLoopRunner(
        RoundTurnQueueBuilder roundTurnQueueBuilder,
        BattleConsoleView battleConsoleView,
        TravelerBasicAttackExecutor travelerBasicAttackExecutor,
        TravelerSkillExecutor travelerSkillExecutor,
        BeastAttackExecutor beastAttackExecutor,
        BattleWinnerEvaluator battleWinnerEvaluator)
    {
        _roundTurnQueueBuilder = roundTurnQueueBuilder;
        _battleConsoleView = battleConsoleView;
        _travelerBasicAttackExecutor = travelerBasicAttackExecutor;
        _travelerSkillExecutor = travelerSkillExecutor;
        _beastAttackExecutor = beastAttackExecutor;
        _battleWinnerEvaluator = battleWinnerEvaluator;
    }

    public void Run(BattleState battleState)
    {
        while (ExecuteRound(battleState))
            StartNextRound(battleState);
    }

    private bool ExecuteRound(BattleState battleState)
    {
        HashSet<TurnParticipantKey> actedParticipants = [];
        PrintRoundStart(battleState, actedParticipants);

        TurnParticipant? participant = GetNextRoundParticipant(battleState, actedParticipants);
        while (participant is not null)
        {
            if (ExecuteTurn(participant, battleState) == TurnExecutionResult.EndBattle)
                return false;

            MarkParticipantAsActed(participant, actedParticipants);

            BattleWinner? winner = GetBattleWinner(battleState);
            if (winner is not null)
            {
                _battleConsoleView.PrintWinner(winner.Value);
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
        RoundTurnQueues initialQueues = _roundTurnQueueBuilder.CreateQueues(battleState, actedParticipants);
        _battleConsoleView.PrintRoundState(battleState, initialQueues);
    }

    private TurnParticipant? GetNextRoundParticipant(
        BattleState battleState,
        IReadOnlySet<TurnParticipantKey> actedParticipants)
    {
        RoundTurnQueues queues = _roundTurnQueueBuilder.CreateQueues(battleState, actedParticipants);
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
        TravelerCombatUnit traveler = battleState.TravelerTeam[participant.BoardSlotIndex];
        TravelerTurnOutcome turnOutcome = _battleConsoleView.RequestTravelerTurn(traveler, battleState);

        if (turnOutcome.Resolution == TravelerTurnResolution.Fled)
            return EndBattleAfterFlee();

        ResolveTravelerAction(traveler, battleState, turnOutcome);
        return TurnExecutionResult.ContinueBattle;
    }

    private TurnExecutionResult EndBattleAfterFlee()
    {
        _battleConsoleView.PrintEnemyWinnerAfterFlee();
        return TurnExecutionResult.EndBattle;
    }

    private void ResolveTravelerAction(TravelerCombatUnit traveler, BattleState battleState, TravelerTurnOutcome turnOutcome)
    {
        switch (turnOutcome.Resolution)
        {
            case TravelerTurnResolution.BasicAttackChosen:
                ExecuteTravelerBasicAttack(traveler, turnOutcome);
                break;
            case TravelerTurnResolution.SkillChosen:
                ExecuteTravelerSkill(traveler, battleState, turnOutcome);
                break;
            case TravelerTurnResolution.DefendChosen:
                ApplyTravelerDefendState(traveler);
                break;
        }
    }

    private void ExecuteTravelerBasicAttack(TravelerCombatUnit traveler, TravelerTurnOutcome turnOutcome)
    {
        if (turnOutcome.SelectedWeapon is null || turnOutcome.SelectedBeastTarget is null)
        {
            return;
        }

        TravelerBasicAttack attack = _travelerBasicAttackExecutor.ExecuteAttack(
            traveler,
            turnOutcome.SelectedBeastTarget,
            turnOutcome.SelectedWeapon);
        _battleConsoleView.PrintTravelerBasicAttack(attack);
    }

    private void ExecuteTravelerSkill(TravelerCombatUnit traveler, BattleState battleState, TravelerTurnOutcome turnOutcome)
    {
        if (turnOutcome.SelectedSkillName is null)
            return;

        TravelerSkillAction action = _travelerSkillExecutor.ExecuteSkill(
            traveler,
            battleState,
            turnOutcome,
            turnOutcome.SelectedSkillName,
            turnOutcome.UsedBp);
        _battleConsoleView.PrintTravelerSkill(action);
    }

    private static void ApplyTravelerDefendState(TravelerCombatUnit traveler)
    {
        traveler.IsDefendingCurrentRound = true;
        traveler.HasPendingDefendPriority = true;
    }

    private BattleWinner? GetBattleWinner(BattleState battleState)
    {
        BattleWinner winner = _battleWinnerEvaluator.GetWinner(battleState);
        return winner == BattleWinner.None ? null : winner;
    }

    private void PrintBattleSnapshotIfRoundContinues(
        BattleState battleState,
        IReadOnlySet<TurnParticipantKey> actedParticipants)
    {
        RoundTurnQueues updatedQueues = _roundTurnQueueBuilder.CreateQueues(battleState, actedParticipants);
        if (updatedQueues.CurrentRound.Count > 0)
            _battleConsoleView.PrintBattleSnapshot(battleState, updatedQueues);
    }

    private TurnExecutionResult ExecuteBeastTurn(TurnParticipant participant, BattleState battleState)
    {
        BeastCombatUnit beast = battleState.BeastTeam[participant.BoardSlotIndex];
        BeastAttack? attack = _beastAttackExecutor.ExecuteAttack(beast, battleState);
        if (attack is not null)
            _battleConsoleView.PrintBeastAttack(attack);

        return TurnExecutionResult.ContinueBattle;
    }

    private static void StartNextRound(BattleState battleState)
    {
        PrepareTravelerRoundStates(battleState);
        PrepareBeastRoundStates(battleState);
        IncreaseAliveTravelerBp(battleState);
        battleState.RoundNumber += 1;
    }

    private static void PrepareTravelerRoundStates(BattleState battleState)
    {
        foreach (TravelerCombatUnit traveler in battleState.TravelerTeam)
        {
            traveler.IsDefendingCurrentRound = false;
            traveler.HasDefendPriorityCurrentRound = traveler.HasPendingDefendPriority;
            traveler.HasPendingDefendPriority = false;
            traveler.HasIncreasedPriorityCurrentRound = traveler.HasPendingIncreasedPriority;
            traveler.HasPendingIncreasedPriority = false;
            traveler.IsWaitingForNextRoundAfterRevive = false;
        }
    }

    private static void PrepareBeastRoundStates(BattleState battleState)
    {
        foreach (BeastCombatUnit beast in battleState.BeastTeam)
        {
            beast.HasRecoveryPriorityCurrentRound = false;

            if (beast.RemainingBreakingRounds > 0)
            {
                beast.RemainingBreakingRounds -= 1;
                if (beast.RemainingBreakingRounds == 0 && beast.IsAlive)
                {
                    beast.CurrentShields = beast.MaxShields;
                    beast.HasRecoveryPriorityCurrentRound = true;
                }
            }

            if (beast.RemainingDecreasedPriorityRounds > 0)
                beast.RemainingDecreasedPriorityRounds -= 1;
        }
    }

    private static void IncreaseAliveTravelerBp(BattleState battleState)
    {
        foreach (TravelerCombatUnit traveler in battleState.TravelerTeam.Where(traveler => traveler.IsAlive))
            traveler.CurrentBp = Math.Min(MaxTravelerBp, traveler.CurrentBp + 1);
    }

    private enum TurnExecutionResult
    {
        ContinueBattle,
        EndBattle
    }
}

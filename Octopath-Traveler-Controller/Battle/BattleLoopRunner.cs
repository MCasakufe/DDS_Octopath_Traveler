using Octopath_Traveler_Models.Battle;
using Octopath_Traveler_View.Battle;

namespace Octopath_Traveler.Battle;

public sealed class BattleLoopRunner
{
    private const int MinActionBp = 0;
    private const int MaxActionBp = 3;
    private const int EvenNumberRemainder = 0;
    private const int EvenNumberDivisor = 2;
    private const int VimAndVigorHealingDivisor = 10;
    private const int SecondWindRecoveryDivisor = 20;
    private const int RoundIncrement = 1;
    private const int MinimumRemainingStatValue = 0;
    private const int MaxTravelerBp = 5;
    private const string PatiencePassiveName = "Patience";

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
        HashSet<int> travelersWithGrantedPatienceTurn = [];
        Queue<int> pendingPatienceTravelerTurns = new();
        PrintRoundStart(battleState, actedParticipants, pendingPatienceTravelerTurns);

        while (true)
        {
            TurnParticipant? participant = GetNextRoundParticipant(
                battleState,
                actedParticipants,
                pendingPatienceTravelerTurns);
            if (participant is null)
            {
                if (ShouldEndRoundWithoutParticipant(
                        battleState,
                        travelersWithGrantedPatienceTurn,
                        pendingPatienceTravelerTurns))
                    return true;

                PrintBattleSnapshotIfRoundContinues(
                    battleState,
                    actedParticipants,
                    pendingPatienceTravelerTurns);
                continue;
            }

            if (ExecuteTurn(participant, battleState) == TurnExecutionResult.EndBattle)
                return false;

            MarkParticipantAsActed(participant, actedParticipants);

            BattleWinner? winner = TryGetBattleWinner(battleState);
            if (winner is not null)
            {
                _battleConsoleView.PrintWinner(winner.Value);
                return false;
            }

            PrintBattleSnapshotIfRoundContinues(
                battleState,
                actedParticipants,
                pendingPatienceTravelerTurns);
        }
    }

    private bool ShouldEndRoundWithoutParticipant(
        BattleState battleState,
        ISet<int> travelersWithGrantedPatienceTurn,
        Queue<int> pendingPatienceTravelerTurns)
        => !TryGrantPatienceExtraTurns(
            battleState,
            travelersWithGrantedPatienceTurn,
            pendingPatienceTravelerTurns);

    private static void MarkParticipantAsActed(
        TurnParticipant participant,
        ISet<TurnParticipantKey> actedParticipants)
        => actedParticipants.Add(new TurnParticipantKey(participant.Side, participant.BoardSlotIndex));

    private void PrintRoundStart(
        BattleState battleState,
        IReadOnlySet<TurnParticipantKey> actedParticipants,
        IReadOnlyCollection<int> pendingPatienceTravelerTurns)
    {
        RoundTurnQueues initialQueues = BuildRoundTurnQueues(
            battleState,
            actedParticipants,
            pendingPatienceTravelerTurns);
        _battleConsoleView.PrintRoundState(battleState, initialQueues);
    }

    private TurnParticipant? GetNextRoundParticipant(
        BattleState battleState,
        IReadOnlySet<TurnParticipantKey> actedParticipants,
        Queue<int> pendingPatienceTravelerTurns)
    {
        while (pendingPatienceTravelerTurns.Count > 0)
        {
            int boardSlotIndex = pendingPatienceTravelerTurns.Dequeue();
            TravelerCombatUnit traveler = battleState.TravelerTeam[boardSlotIndex];
            if (!traveler.IsAlive)
                continue;

            return BuildTravelerTurnParticipant(traveler);
        }

        RoundTurnQueues queues = _roundTurnQueueBuilder.BuildRoundTurnQueues(battleState, actedParticipants);
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

        int usedBp = CalculateUsableBpForAction(traveler, turnOutcome.UsedBp);
        ConsumeTravelerBp(traveler, usedBp);

        TravelerBasicAttack attack = _travelerBasicAttackExecutor.ExecuteAttack(new TravelerBasicAttackExecutionRequest(
            traveler,
            turnOutcome.SelectedBeastTarget,
            turnOutcome.SelectedWeapon,
            usedBp));
        _battleConsoleView.PrintTravelerBasicAttack(attack);
    }

    private void ExecuteTravelerSkill(TravelerCombatUnit traveler, BattleState battleState, TravelerTurnOutcome turnOutcome)
    {
        if (turnOutcome.SelectedSkillName is null)
            return;

        int usedBp = CalculateUsableBpForAction(traveler, turnOutcome.UsedBp);
        ConsumeTravelerBp(traveler, usedBp);

        TravelerSkillAction action = _travelerSkillExecutor.ExecuteSkill(new TravelerSkillExecutionRequest(
            traveler,
            battleState,
            turnOutcome,
            turnOutcome.SelectedSkillName));
        _battleConsoleView.PrintTravelerSkill(action);
    }

    private static void ApplyTravelerDefendState(TravelerCombatUnit traveler)
    {
        traveler.IsDefendingCurrentRound = true;
        traveler.HasPendingDefendPriority = true;
    }

    private static int CalculateUsableBpForAction(TravelerCombatUnit traveler, int requestedBp)
    {
        int cappedRequestedBp = Math.Clamp(requestedBp, MinActionBp, MaxActionBp);
        return Math.Min(traveler.CurrentBp, cappedRequestedBp);
    }

    private static void ConsumeTravelerBp(TravelerCombatUnit traveler, int usedBp)
    {
        traveler.CurrentBp = Math.Max(MinimumRemainingStatValue, traveler.CurrentBp - usedBp);
        if (usedBp > MinActionBp)
            traveler.SpentBpThisRound = true;
    }

    private BattleWinner? TryGetBattleWinner(BattleState battleState)
    {
        BattleWinner winner = _battleWinnerEvaluator.EvaluateWinner(battleState);
        return winner == BattleWinner.None ? null : winner;
    }

    private void PrintBattleSnapshotIfRoundContinues(
        BattleState battleState,
        IReadOnlySet<TurnParticipantKey> actedParticipants,
        IReadOnlyCollection<int> pendingPatienceTravelerTurns)
    {
        RoundTurnQueues updatedQueues = BuildRoundTurnQueues(
            battleState,
            actedParticipants,
            pendingPatienceTravelerTurns);
        if (updatedQueues.CurrentRound.Count > 0)
            _battleConsoleView.PrintBattleSnapshot(battleState, updatedQueues);
    }

    private RoundTurnQueues BuildRoundTurnQueues(
        BattleState battleState,
        IReadOnlySet<TurnParticipantKey> actedParticipants,
        IReadOnlyCollection<int> pendingPatienceTravelerTurns)
    {
        RoundTurnQueues baseQueues = _roundTurnQueueBuilder.BuildRoundTurnQueues(battleState, actedParticipants);
        if (pendingPatienceTravelerTurns.Count == 0)
            return baseQueues;

        List<TurnParticipant> pendingTravelerParticipants = pendingPatienceTravelerTurns
            .Select(boardSlotIndex => battleState.TravelerTeam[boardSlotIndex])
            .Where(traveler => traveler.IsAlive)
            .Select(BuildTravelerTurnParticipant)
            .ToList();

        List<TurnParticipant> currentQueue = pendingTravelerParticipants
            .Concat(baseQueues.CurrentRound)
            .ToList();
        return new RoundTurnQueues(currentQueue, baseQueues.NextRound);
    }

    private bool TryGrantPatienceExtraTurns(
        BattleState battleState,
        ISet<int> travelersWithGrantedPatienceTurn,
        Queue<int> pendingPatienceTravelerTurns)
    {
        List<TravelerCombatUnit> eligibleTravelers = battleState.TravelerTeam
            .Where(traveler => IsEligibleForPatienceExtraTurn(traveler, travelersWithGrantedPatienceTurn))
            .OrderBy(traveler => traveler.BoardSlotIndex)
            .ToList();
        if (eligibleTravelers.Count == 0)
            return false;

        foreach (TravelerCombatUnit traveler in eligibleTravelers)
        {
            travelersWithGrantedPatienceTurn.Add(traveler.BoardSlotIndex);
            pendingPatienceTravelerTurns.Enqueue(traveler.BoardSlotIndex);
            _battleConsoleView.PrintPatienceExtraTurn(traveler.Name);
        }

        return true;
    }

    private static bool IsEligibleForPatienceExtraTurn(
        TravelerCombatUnit traveler,
        ISet<int> travelersWithGrantedPatienceTurn)
        => traveler.IsAlive
           && HasPatiencePassive(traveler)
           && !travelersWithGrantedPatienceTurn.Contains(traveler.BoardSlotIndex)
           && HasEvenCurrentHpAndSp(traveler);

    private static bool HasEvenCurrentHpAndSp(TravelerCombatUnit traveler)
        => IsEven(traveler.CurrentHp) && IsEven(traveler.CurrentSp);

    private static bool HasPatiencePassive(TravelerCombatUnit traveler)
        => traveler.AssignedPassiveSkillNames.Contains(PatiencePassiveName, StringComparer.Ordinal);

    private static bool IsEven(int value)
        => value % EvenNumberDivisor == EvenNumberRemainder;

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
        ApplyTravelerRoundEndPassiveRecovery(battleState);
        PrepareTravelerRoundStates(battleState);
        PrepareBeastRoundStates(battleState);
        IncreaseAliveTravelerBp(battleState);
        battleState.RoundNumber += RoundIncrement;
    }

    private static void ApplyTravelerRoundEndPassiveRecovery(BattleState battleState)
    {
        foreach (TravelerCombatUnit traveler in battleState.TravelerTeam.Where(traveler => traveler.IsAlive))
        {
            if (traveler.HasVimAndVigor)
                traveler.CurrentHp = Math.Min(traveler.MaxHp, traveler.CurrentHp + traveler.MaxHp / VimAndVigorHealingDivisor);

            if (traveler.HasSecondWind)
                traveler.CurrentSp = Math.Min(traveler.MaxSp, traveler.CurrentSp + traveler.MaxSp / SecondWindRecoveryDivisor);
        }
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
            ResetBeastRecoveryPriority(beast);
            UpdateBeastBreakingStateForNextRound(beast);
            DecreaseBeastPriorityPenalty(beast);
        }
    }

    private static void ResetBeastRecoveryPriority(BeastCombatUnit beast)
        => beast.HasRecoveryPriorityCurrentRound = false;

    private static void UpdateBeastBreakingStateForNextRound(BeastCombatUnit beast)
    {
        if (!HasBreakingRoundsRemaining(beast))
            return;

        beast.RemainingBreakingRounds -= 1;
        if (ShouldRecoverFromBreakingState(beast))
            RecoverBeastShieldsAfterBreaking(beast);
    }

    private static bool HasBreakingRoundsRemaining(BeastCombatUnit beast)
        => beast.RemainingBreakingRounds > 0;

    private static bool ShouldRecoverFromBreakingState(BeastCombatUnit beast)
        => beast.RemainingBreakingRounds == 0 && beast.IsAlive;

    private static void RecoverBeastShieldsAfterBreaking(BeastCombatUnit beast)
    {
        beast.CurrentShields = beast.MaxShields;
        beast.HasRecoveryPriorityCurrentRound = true;
    }

    private static void DecreaseBeastPriorityPenalty(BeastCombatUnit beast)
    {
        if (beast.RemainingDecreasedPriorityRounds > 0)
            beast.RemainingDecreasedPriorityRounds -= 1;
    }

    private static void IncreaseAliveTravelerBp(BattleState battleState)
    {
        foreach (TravelerCombatUnit traveler in battleState.TravelerTeam.Where(traveler => traveler.IsAlive))
        {
            if (!traveler.SpentBpThisRound)
                traveler.CurrentBp = Math.Min(MaxTravelerBp, traveler.CurrentBp + 1);
        }

        foreach (TravelerCombatUnit traveler in battleState.TravelerTeam)
            traveler.SpentBpThisRound = false;
    }

    private enum TurnExecutionResult
    {
        ContinueBattle,
        EndBattle
    }
}

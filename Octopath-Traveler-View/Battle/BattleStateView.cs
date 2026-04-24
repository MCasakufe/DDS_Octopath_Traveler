using Octopath_Traveler_Models.Battle;

namespace Octopath_Traveler_View.Battle;

public sealed class BattleStateView
{
    private const string SeparatorLine = "----------------------------------------";

    private readonly View _view;

    public BattleStateView(View view)
    {
        _view = view;
    }

    public void PrintRoundState(BattleState battleState, RoundTurnQueues roundTurnQueues)
    {
        WriteRoundHeader(battleState.RoundNumber);
        WriteTeamState(battleState);
        WriteQueueBlock("Turnos de la ronda", roundTurnQueues.CurrentRound);
        WriteQueueBlock("Turnos de la siguiente ronda", roundTurnQueues.NextRound);
    }

    public void PrintBattleSnapshot(BattleState battleState, RoundTurnQueues roundTurnQueues)
    {
        _view.WriteLine(SeparatorLine);
        WriteTeamState(battleState);
        WriteQueueBlock("Turnos de la ronda", roundTurnQueues.CurrentRound);
        WriteQueueBlock("Turnos de la siguiente ronda", roundTurnQueues.NextRound);
    }

    private void WriteRoundHeader(int roundNumber)
    {
        _view.WriteLine(SeparatorLine);
        _view.WriteLine($"INICIA RONDA {roundNumber}");
        _view.WriteLine(SeparatorLine);
    }

    private void WriteTeamState(BattleState battleState)
    {
        _view.WriteLine("Equipo del jugador");
        foreach (var traveler in battleState.TravelerTeam)
            _view.WriteLine(BuildTravelerLine(traveler));

        _view.WriteLine("Equipo del enemigo");
        foreach (var beast in battleState.BeastTeam)
            _view.WriteLine(BuildBeastLine(beast));
    }

    private void WriteQueueBlock(string title, IReadOnlyList<TurnParticipant> turnQueue)
    {
        _view.WriteLine(SeparatorLine);
        _view.WriteLine(title);

        for (var index = 0; index < turnQueue.Count; index++)
            _view.WriteLine($"{index + 1}.{turnQueue[index].Name}");
    }

    private static string BuildTravelerLine(TravelerCombatUnit traveler)
        => $"{GetSlotLetter(traveler.BoardSlotIndex)}-{traveler.Name} - HP:{traveler.CurrentHp}/{traveler.MaxHp} SP:{traveler.CurrentSp}/{traveler.MaxSp} BP:{traveler.CurrentBp}";

    private static string BuildBeastLine(BeastCombatUnit beast)
        => $"{GetSlotLetter(beast.BoardSlotIndex)}-{beast.Name} - HP:{beast.CurrentHp}/{beast.MaxHp} Shields:{beast.CurrentShields}";

    private static char GetSlotLetter(int boardSlotIndex)
        => (char)('A' + boardSlotIndex);
}

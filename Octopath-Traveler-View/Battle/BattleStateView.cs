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

    public void WriteRoundState(BattleState battleState, RoundTurnQueues roundTurnQueues)
    {
        WriteRoundHeader(battleState.RoundNumber);
        WriteTeamStatusBlock(battleState);
        WriteTurnQueueSection("Turnos de la ronda", roundTurnQueues.CurrentRound);
        WriteTurnQueueSection("Turnos de la siguiente ronda", roundTurnQueues.NextRound);
    }

    public void WriteBattleSnapshot(BattleState battleState, RoundTurnQueues roundTurnQueues)
    {
        _view.WriteLine(SeparatorLine);
        WriteTeamStatusBlock(battleState);
        WriteTurnQueueSection("Turnos de la ronda", roundTurnQueues.CurrentRound);
        WriteTurnQueueSection("Turnos de la siguiente ronda", roundTurnQueues.NextRound);
    }

    private void WriteRoundHeader(int roundNumber)
    {
        _view.WriteLine(SeparatorLine);
        _view.WriteLine($"INICIA RONDA {roundNumber}");
        _view.WriteLine(SeparatorLine);
    }

    private void WriteTeamStatusBlock(BattleState battleState)
    {
        _view.WriteLine("Equipo del jugador");
        foreach (TravelerCombatUnit traveler in battleState.TravelerTeam)
            _view.WriteLine(BuildTravelerLine(traveler));

        _view.WriteLine("Equipo del enemigo");
        foreach (BeastCombatUnit beast in battleState.BeastTeam)
            _view.WriteLine(BuildBeastLine(beast));
    }

    private void WriteTurnQueueSection(string title, IReadOnlyList<TurnParticipant> turnQueue)
    {
        _view.WriteLine(SeparatorLine);
        _view.WriteLine(title);

        for (int index = 0; index < turnQueue.Count; index++)
            _view.WriteLine($"{index + 1}.{turnQueue[index].Name}");
    }

    private static string BuildTravelerLine(TravelerCombatUnit traveler)
        => $"{BuildUnitLabel(traveler.BoardSlotIndex, traveler.Name)} - "
           + $"HP:{traveler.CurrentHp}/{traveler.MaxHp} "
           + $"SP:{traveler.CurrentSp}/{traveler.MaxSp} "
           + $"BP:{traveler.CurrentBp}";

    private static string BuildBeastLine(BeastCombatUnit beast)
        => $"{BuildUnitLabel(beast.BoardSlotIndex, beast.Name)} - "
           + $"HP:{beast.CurrentHp}/{beast.MaxHp} Shields:{beast.CurrentShields}";

    private static string BuildUnitLabel(int boardSlotIndex, string unitName)
        => $"{ConvertSlotIndexToLetter(boardSlotIndex)}-{unitName}";

    private static char ConvertSlotIndexToLetter(int boardSlotIndex)
        => (char)('A' + boardSlotIndex);
}

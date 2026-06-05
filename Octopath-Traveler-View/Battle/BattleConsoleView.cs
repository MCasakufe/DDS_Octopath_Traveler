using Octopath_Traveler_Models.Battle;

namespace Octopath_Traveler_View.Battle;

public sealed class BattleConsoleView
{
    private const string SeparatorLine = "----------------------------------------";

    private readonly View _view;
    private readonly BattleStateView _battleStateView;
    private readonly BattleActionView _battleActionView;
    private readonly TravelerTurnInputView _travelerTurnInputView;

    public BattleConsoleView(View view)
    {
        _view = view;
        _battleStateView = new BattleStateView(view);
        _battleActionView = new BattleActionView(view);
        _travelerTurnInputView = new TravelerTurnInputView(view);
    }

    public TravelerTurnOutcome RequestTravelerTurn(TravelerCombatUnit traveler, BattleState battleState)
        => _travelerTurnInputView.RequestTurn(traveler, battleState);

    public void WriteRoundState(BattleState battleState, RoundTurnQueues roundTurnQueues)
        => _battleStateView.WriteRoundState(battleState, roundTurnQueues);

    public void WriteBattleSnapshot(BattleState battleState, RoundTurnQueues roundTurnQueues)
        => _battleStateView.WriteBattleSnapshot(battleState, roundTurnQueues);

    public void WriteTravelerBasicAttack(TravelerBasicAttack attack)
        => _battleActionView.WriteTravelerBasicAttack(attack);

    public void WriteTravelerSkill(TravelerSkillAction action)
        => _battleActionView.WriteTravelerSkill(action);

    public void WriteBeastAttack(BeastAttack attack)
        => _battleActionView.WriteBeastAttack(attack);

    public void WritePatienceExtraTurn(string travelerName)
        => _battleActionView.WritePatienceExtraTurn(travelerName);

    public void WriteWinner(BattleWinner winner)
    {
        if (winner == BattleWinner.None)
            return;

        _view.WriteLine(SeparatorLine);
        _view.WriteLine(winner == BattleWinner.TravelerTeam
            ? "Gana equipo del jugador"
            : "Gana equipo del enemigo");
    }

    public void WriteEnemyWinnerAfterFlee()
    {
        _view.WriteLine(SeparatorLine);
        _view.WriteLine("El equipo de viajeros ha huido!");
        _view.WriteLine(SeparatorLine);
        _view.WriteLine("Gana equipo del enemigo");
    }
}

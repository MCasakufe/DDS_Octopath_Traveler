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

    public void PrintRoundState(BattleState battleState, RoundTurnQueues roundTurnQueues)
        => _battleStateView.PrintRoundState(battleState, roundTurnQueues);

    public void PrintBattleSnapshot(BattleState battleState, RoundTurnQueues roundTurnQueues)
        => _battleStateView.PrintBattleSnapshot(battleState, roundTurnQueues);

    public void PrintTravelerBasicAttack(TravelerBasicAttack attack)
        => _battleActionView.PrintTravelerBasicAttack(attack);

    public void PrintTravelerSkill(TravelerSkillAction action)
        => _battleActionView.PrintTravelerSkill(action);

    public void PrintBeastAttack(BeastAttack attack)
        => _battleActionView.PrintBeastAttack(attack);

    public void PrintWinner(BattleWinner winner)
    {
        if (winner == BattleWinner.None)
            return;

        _view.WriteLine(SeparatorLine);
        _view.WriteLine(winner == BattleWinner.TravelerTeam
            ? "Gana equipo del jugador"
            : "Gana equipo del enemigo");
    }

    public void PrintEnemyWinnerAfterFlee()
    {
        _view.WriteLine(SeparatorLine);
        _view.WriteLine("El equipo de viajeros ha huido!");
        _view.WriteLine(SeparatorLine);
        _view.WriteLine("Gana equipo del enemigo");
    }
}

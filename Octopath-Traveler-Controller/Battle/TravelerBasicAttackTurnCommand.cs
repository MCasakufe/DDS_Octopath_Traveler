using Octopath_Traveler_Models.Battle;
using Octopath_Traveler_View.Battle;

namespace Octopath_Traveler.Battle;

public sealed class TravelerBasicAttackTurnCommand
{
    private readonly TravelerBasicAttackExecutor _travelerBasicAttackExecutor;
    private readonly BattleConsoleView _battleConsoleView;

    public TravelerBasicAttackTurnCommand(
        TravelerBasicAttackExecutor travelerBasicAttackExecutor,
        BattleConsoleView battleConsoleView)
    {
        _travelerBasicAttackExecutor = travelerBasicAttackExecutor;
        _battleConsoleView = battleConsoleView;
    }

    internal void Execute(TravelerCombatUnit traveler, TravelerTurnOutcome turnOutcome)
    {
        if (turnOutcome.SelectedWeapon is null || turnOutcome.SelectedBeastTarget is null)
            return;

        int usedBp = traveler.ConsumeActionBp(turnOutcome.UsedBp);
        TravelerBasicAttack attack = _travelerBasicAttackExecutor.ExecuteAttack(new TravelerBasicAttackExecutionRequest(
            traveler,
            turnOutcome.SelectedBeastTarget,
            turnOutcome.SelectedWeapon,
            usedBp));
        _battleConsoleView.PrintTravelerBasicAttack(attack);
    }
}

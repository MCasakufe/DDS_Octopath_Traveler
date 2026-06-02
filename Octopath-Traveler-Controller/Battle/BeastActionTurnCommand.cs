using Octopath_Traveler_Models.Battle;
using Octopath_Traveler_View.Battle;

namespace Octopath_Traveler.Battle;

public sealed class BeastActionTurnCommand
{
    private readonly BeastAttackExecutor _beastAttackExecutor;
    private readonly BattleConsoleView _battleConsoleView;

    public BeastActionTurnCommand(
        BeastAttackExecutor beastAttackExecutor,
        BattleConsoleView battleConsoleView)
    {
        _beastAttackExecutor = beastAttackExecutor;
        _battleConsoleView = battleConsoleView;
    }

    internal void Execute(BeastCombatUnit beast, BattleState battleState)
    {
        BeastAttack? attack = _beastAttackExecutor.ExecuteAttack(beast, battleState);
        if (attack is not null)
            _battleConsoleView.PrintBeastAttack(attack);
    }
}

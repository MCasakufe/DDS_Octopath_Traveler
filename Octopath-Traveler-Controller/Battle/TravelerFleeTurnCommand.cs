using Octopath_Traveler_View.Battle;

namespace Octopath_Traveler.Battle;

public sealed class TravelerFleeTurnCommand
{
    private readonly BattleConsoleView _battleConsoleView;

    public TravelerFleeTurnCommand(BattleConsoleView battleConsoleView)
    {
        _battleConsoleView = battleConsoleView;
    }

    internal void Execute()
        => _battleConsoleView.WriteEnemyWinnerAfterFlee();
}

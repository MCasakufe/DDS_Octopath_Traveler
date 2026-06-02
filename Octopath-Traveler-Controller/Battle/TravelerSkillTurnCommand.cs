using Octopath_Traveler_Models.Battle;
using Octopath_Traveler_View.Battle;

namespace Octopath_Traveler.Battle;

public sealed class TravelerSkillTurnCommand
{
    private readonly TravelerSkillExecutor _travelerSkillExecutor;
    private readonly BattleConsoleView _battleConsoleView;

    public TravelerSkillTurnCommand(
        TravelerSkillExecutor travelerSkillExecutor,
        BattleConsoleView battleConsoleView)
    {
        _travelerSkillExecutor = travelerSkillExecutor;
        _battleConsoleView = battleConsoleView;
    }

    internal void Execute(TravelerCombatUnit traveler, BattleState battleState, TravelerTurnOutcome turnOutcome)
    {
        if (turnOutcome.SelectedSkillName is null)
            return;

        traveler.ConsumeActionBp(turnOutcome.UsedBp);
        TravelerSkillAction action = _travelerSkillExecutor.ExecuteSkill(new TravelerSkillExecutionRequest(
            traveler,
            battleState,
            turnOutcome,
            turnOutcome.SelectedSkillName));
        _battleConsoleView.PrintTravelerSkill(action);
    }
}

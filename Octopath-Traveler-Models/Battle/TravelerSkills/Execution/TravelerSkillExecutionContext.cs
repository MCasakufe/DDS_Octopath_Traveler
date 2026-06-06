using Octopath_Traveler_Models.RuntimeData;

namespace Octopath_Traveler_Models.Battle;

internal sealed record TravelerSkillExecutionContext(
    TravelerSkillExecutionRequest SkillExecutionRequest,
    SkillDefinition SelectedSkill)
{
    public TravelerCombatUnit Traveler => SkillExecutionRequest.Traveler;
    public BattleState BattleState => SkillExecutionRequest.BattleState;
    public TravelerTurnOutcome TurnOutcome => SkillExecutionRequest.TurnOutcome;
    public string SkillName => SkillExecutionRequest.SkillName;

    public TravelerSkillTargetSelectionContext BuildTargetSelectionContext()
        => new(Traveler, BattleState, TurnOutcome, SelectedSkill);
}

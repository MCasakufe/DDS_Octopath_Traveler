namespace Octopath_Traveler_Models.Battle;

public sealed record TravelerSkillExecutionRequest(
    TravelerCombatUnit Traveler,
    BattleState BattleState,
    TravelerTurnOutcome TurnOutcome,
    string SkillName);

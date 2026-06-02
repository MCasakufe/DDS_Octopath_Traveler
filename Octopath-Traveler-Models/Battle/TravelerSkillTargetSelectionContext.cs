using Octopath_Traveler_Models.RuntimeData;

namespace Octopath_Traveler_Models.Battle;

internal sealed record TravelerSkillTargetSelectionContext(
    TravelerCombatUnit Traveler,
    BattleState BattleState,
    TravelerTurnOutcome TurnOutcome,
    SkillDefinition Skill);

namespace Octopath_Traveler_Models.Battle;

internal sealed record BeastAttackTargetSelectionRequest(
    string SkillName,
    string TargetType,
    BattleState BattleState);

namespace Octopath_Traveler_Models.Battle;

public sealed record BeastAttack(
    string AttackerName,
    string SkillName,
    IReadOnlyList<BeastAttackResult> Results);

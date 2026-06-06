namespace Octopath_Traveler_Models.Battle;

public sealed record BeastAttackHpSummaryResult(
    string TargetName,
    int CurrentHp)
    : BeastAttackResult;

namespace Octopath_Traveler_Models.Battle;

public abstract record BeastAttackResult;

public sealed record BeastAttackDefendResult(string TargetName)
    : BeastAttackResult;

public sealed record BeastAttackDamageResult(
    string TargetName,
    int Damage,
    BeastAttackDamageKind DamageKind)
    : BeastAttackResult;

public sealed record BeastAttackHpSummaryResult(
    string TargetName,
    int CurrentHp)
    : BeastAttackResult;

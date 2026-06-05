namespace Octopath_Traveler_Models.Battle;

public sealed record BeastAttackDamageResult(
    string TargetName,
    int Damage,
    BeastAttackDamageKind DamageKind)
    : BeastAttackResult;

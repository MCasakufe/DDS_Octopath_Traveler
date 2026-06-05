namespace Octopath_Traveler_Models.Battle;

public sealed record BeastAttackStatusEffectResult(
    string TargetName,
    UnitStatusEffectKind StatusEffectKind,
    int DurationRounds)
    : BeastAttackResult;

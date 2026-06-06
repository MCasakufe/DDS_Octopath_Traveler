namespace Octopath_Traveler_Models.Battle;

public sealed record TravelerSkillStatusEffectResult(
    string TargetName,
    UnitStatusEffectKind StatusEffectKind,
    int DurationRounds)
    : TravelerSkillResult;

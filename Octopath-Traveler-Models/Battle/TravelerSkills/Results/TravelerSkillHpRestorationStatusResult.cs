namespace Octopath_Traveler_Models.Battle;

public sealed record TravelerSkillHpRestorationStatusResult(
    string TargetName,
    int DurationRounds)
    : TravelerSkillResult;

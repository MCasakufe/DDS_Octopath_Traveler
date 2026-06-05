namespace Octopath_Traveler_Models.Battle;

public sealed record TravelerSkillActivationDuplicationStatusResult(
    string TargetName,
    int DurationRounds)
    : TravelerSkillResult;

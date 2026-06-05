namespace Octopath_Traveler_Models.Battle;

public sealed record TravelerSkillTargetModificationStatusResult(
    string TargetName,
    int DurationRounds)
    : TravelerSkillResult;

namespace Octopath_Traveler_Models.Battle;

public sealed record TravelerSkillPriorityChangeResult(
    string TargetName,
    int DurationRounds) : TravelerSkillResult;

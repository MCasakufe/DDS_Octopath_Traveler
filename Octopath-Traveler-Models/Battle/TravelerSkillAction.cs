namespace Octopath_Traveler_Models.Battle;

public sealed record TravelerSkillAction(
    string TravelerName,
    string SkillName,
    IReadOnlyList<TravelerSkillResult> Results);

namespace Octopath_Traveler_Models.TeamSelection;

public sealed record TravelerSetup(
    string Name,
    IReadOnlyList<string> ActiveSkills,
    IReadOnlyList<string> PassiveSkills);

namespace Octopath_Traveler_Models.TeamSelection;

public sealed record ValidationCatalog(
    IReadOnlySet<string> ValidTravelerNames,
    IReadOnlySet<string> ValidBeastNames,
    IReadOnlySet<string> ValidActiveSkillNames,
    IReadOnlySet<string> ValidPassiveSkillNames);


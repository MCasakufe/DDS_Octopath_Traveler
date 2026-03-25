namespace Octopath_Traveler.TeamSelection;

public sealed record ValidationCatalog(
    IReadOnlySet<string> ValidTravelerNames,
    IReadOnlySet<string> ValidBeastNames,
    IReadOnlySet<string> ValidActiveSkillNames,
    IReadOnlySet<string> ValidPassiveSkillNames);

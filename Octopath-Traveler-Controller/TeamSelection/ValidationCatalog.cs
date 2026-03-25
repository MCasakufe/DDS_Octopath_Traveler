namespace Octopath_Traveler.TeamSelection;

public sealed record ValidationCatalog(
    HashSet<string> ValidTravelerNames,
    HashSet<string> ValidBeastNames,
    HashSet<string> ValidActiveSkillNames,
    HashSet<string> ValidPassiveSkillNames);

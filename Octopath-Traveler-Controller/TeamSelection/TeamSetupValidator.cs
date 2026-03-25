namespace Octopath_Traveler.TeamSelection;

public sealed class TeamSetupValidator
{
    private readonly JsonValidationCatalogProvider _catalogProvider;

    public TeamSetupValidator(JsonValidationCatalogProvider catalogProvider)
    {
        _catalogProvider = catalogProvider;
    }

    public bool IsValid(TeamSetup teamSetup)
    {
        var catalog = _catalogProvider.TryLoad();
        if (catalog is null)
            return false;

        return HasValidTeamMemberCounts(teamSetup)
               && HasNoDuplicateTravelerNames(teamSetup)
               && HasNoDuplicateBeastNames(teamSetup)
               && TravelersExist(teamSetup, catalog.ValidTravelerNames)
               && BeastsExist(teamSetup, catalog.ValidBeastNames)
               && TravelerSkillsAreValid(teamSetup, catalog.ValidActiveSkillNames, catalog.ValidPassiveSkillNames);
    }

    private static bool HasValidTeamMemberCounts(TeamSetup teamSetup)
        => teamSetup.Travelers.Count is >= 1 and <= 4
           && teamSetup.Beasts.Count is >= 1 and <= 5;

    private static bool HasNoDuplicateTravelerNames(TeamSetup teamSetup)
        => HasNoDuplicateNames(teamSetup.Travelers.Select(traveler => traveler.Name));

    private static bool HasNoDuplicateBeastNames(TeamSetup teamSetup)
        => HasNoDuplicateNames(teamSetup.Beasts);

    private static bool HasNoDuplicateNames(IEnumerable<string> names)
    {
        var uniqueNames = new HashSet<string>(StringComparer.Ordinal);
        foreach (var name in names)
        {
            if (!uniqueNames.Add(name))
                return false;
        }

        return true;
    }

    private static bool TravelersExist(TeamSetup teamSetup, HashSet<string> validTravelerNames)
        => teamSetup.Travelers.All(traveler => validTravelerNames.Contains(traveler.Name));

    private static bool BeastsExist(TeamSetup teamSetup, HashSet<string> validBeastNames)
        => teamSetup.Beasts.All(beast => validBeastNames.Contains(beast));

    private static bool TravelerSkillsAreValid(
        TeamSetup teamSetup,
        HashSet<string> validActiveSkillNames,
        HashSet<string> validPassiveSkillNames)
        => teamSetup.Travelers.All(traveler =>
            HasValidSkillCounts(traveler)
            && HasNoDuplicateNames(traveler.ActiveSkills)
            && HasNoDuplicateNames(traveler.PassiveSkills)
            && SkillsExist(traveler.ActiveSkills, validActiveSkillNames)
            && SkillsExist(traveler.PassiveSkills, validPassiveSkillNames));

    private static bool HasValidSkillCounts(TravelerSetup traveler)
        => traveler.ActiveSkills.Count <= 8
           && traveler.PassiveSkills.Count <= 4;

    private static bool SkillsExist(IEnumerable<string> selectedSkillNames, HashSet<string> validSkillNames)
        => selectedSkillNames.All(validSkillNames.Contains);
}

namespace Octopath_Traveler_Models.TeamSelection;

public sealed class TeamSetupValidator
{
    private const int MinTravelerCount = 1;
    private const int MaxTravelerCount = 4;
    private const int MinBeastCount = 1;
    private const int MaxBeastCount = 5;
    private const int MaxActiveSkillCount = 8;
    private const int MaxPassiveSkillCount = 4;

    private readonly JsonValidationCatalogProvider _catalogProvider;

    public TeamSetupValidator(JsonValidationCatalogProvider catalogProvider)
    {
        _catalogProvider = catalogProvider;
    }

    public bool IsValid(TeamSetup teamSetup)
    {
        ValidationCatalog catalog = _catalogProvider.Load();

        return HasValidTeamMemberCounts(teamSetup)
               && HasNoDuplicateTravelerNames(teamSetup)
               && HasNoDuplicateBeastNames(teamSetup)
               && TravelersExist(teamSetup, catalog.ValidTravelerNames)
               && BeastsExist(teamSetup, catalog.ValidBeastNames)
               && TravelerSkillsAreValid(teamSetup, catalog.ValidActiveSkillNames, catalog.ValidPassiveSkillNames);
    }

    private static bool HasValidTeamMemberCounts(TeamSetup teamSetup)
        => teamSetup.Travelers.Count is >= MinTravelerCount and <= MaxTravelerCount
           && teamSetup.Beasts.Count is >= MinBeastCount and <= MaxBeastCount;

    private static bool HasNoDuplicateTravelerNames(TeamSetup teamSetup)
        => HasNoDuplicateNames(teamSetup.Travelers.Select(traveler => traveler.Name));

    private static bool HasNoDuplicateBeastNames(TeamSetup teamSetup)
        => HasNoDuplicateNames(teamSetup.Beasts);

    private static bool HasNoDuplicateNames(IEnumerable<string> names)
    {
        HashSet<string> uniqueNames = new(StringComparer.Ordinal);
        foreach (string name in names)
        {
            if (!uniqueNames.Add(name))
                return false;
        }

        return true;
    }

    private static bool TravelersExist(TeamSetup teamSetup, IReadOnlySet<string> validTravelerNames)
        => SelectedNamesExist(teamSetup.Travelers.Select(traveler => traveler.Name), validTravelerNames);

    private static bool BeastsExist(TeamSetup teamSetup, IReadOnlySet<string> validBeastNames)
        => SelectedNamesExist(teamSetup.Beasts, validBeastNames);

    private static bool TravelerSkillsAreValid(
        TeamSetup teamSetup,
        IReadOnlySet<string> validActiveSkillNames,
        IReadOnlySet<string> validPassiveSkillNames)
        => teamSetup.Travelers.All(traveler =>
            HasValidSkillCounts(traveler)
            && HasNoDuplicateNames(traveler.ActiveSkills)
            && HasNoDuplicateNames(traveler.PassiveSkills)
            && SelectedNamesExist(traveler.ActiveSkills, validActiveSkillNames)
            && SelectedNamesExist(traveler.PassiveSkills, validPassiveSkillNames));

    private static bool HasValidSkillCounts(TravelerSetup traveler)
        => traveler.ActiveSkills.Count <= MaxActiveSkillCount
           && traveler.PassiveSkills.Count <= MaxPassiveSkillCount;

    private static bool SelectedNamesExist(IEnumerable<string> selectedNames, IReadOnlySet<string> validNames)
        => selectedNames.All(validNames.Contains);
}


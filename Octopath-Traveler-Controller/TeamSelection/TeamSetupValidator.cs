namespace Octopath_Traveler.TeamSelection;

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
        var catalog = _catalogProvider.Load();

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
        var uniqueNames = new HashSet<string>(StringComparer.Ordinal);
        foreach (var name in names)
        {
            if (!uniqueNames.Add(name))
                return false;
        }

        return true;
    }

    private static bool TravelersExist(TeamSetup teamSetup, IReadOnlySet<string> validTravelerNames)
        => teamSetup.Travelers.All(traveler => validTravelerNames.Contains(traveler.Name));

    private static bool BeastsExist(TeamSetup teamSetup, IReadOnlySet<string> validBeastNames)
        => teamSetup.Beasts.All(beast => validBeastNames.Contains(beast));

    private static bool TravelerSkillsAreValid(
        TeamSetup teamSetup,
        IReadOnlySet<string> validActiveSkillNames,
        IReadOnlySet<string> validPassiveSkillNames)
        => teamSetup.Travelers.All(traveler =>
            HasValidSkillCounts(traveler)
            && HasNoDuplicateNames(traveler.ActiveSkills)
            && HasNoDuplicateNames(traveler.PassiveSkills)
            && SkillsExist(traveler.ActiveSkills, validActiveSkillNames)
            && SkillsExist(traveler.PassiveSkills, validPassiveSkillNames));

    private static bool HasValidSkillCounts(TravelerSetup traveler)
        => traveler.ActiveSkills.Count <= MaxActiveSkillCount
           && traveler.PassiveSkills.Count <= MaxPassiveSkillCount;

    private static bool SkillsExist(IEnumerable<string> selectedSkillNames, IReadOnlySet<string> validSkillNames)
        => selectedSkillNames.All(validSkillNames.Contains);
}

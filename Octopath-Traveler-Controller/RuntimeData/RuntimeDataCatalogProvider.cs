using System.Text.Json;

namespace Octopath_Traveler.RuntimeData;

public sealed class RuntimeDataCatalogProvider
{
    private const string CharactersFileName = "characters.json";
    private const string EnemiesFileName = "enemies.json";
    private const string ActiveSkillsFileName = "skills.json";
    private const string PassiveSkillsFileName = "passive_skills.json";
    private const string BeastSkillsFileName = "beast_skills.json";

    private const string NamePropertyName = "Name";
    private const string StatsPropertyName = "Stats";
    private const string HpPropertyName = "HP";
    private const string SpPropertyName = "SP";
    private const string PhysAtkPropertyName = "PhysAtk";
    private const string PhysDefPropertyName = "PhysDef";
    private const string SpeedPropertyName = "Speed";
    private const string WeaponsPropertyName = "Weapons";
    private const string ShieldsPropertyName = "Shields";
    private const string SkillPropertyName = "Skill";

    private readonly string _dataFolderPath;

    public RuntimeDataCatalogProvider(string teamsFolder)
    {
        _dataFolderPath = Path.GetDirectoryName(teamsFolder) ?? string.Empty;
    }

    public RuntimeDataCatalog? TryLoad()
    {
        var travelersByName = TryLoadTravelerDefinitions();
        var beastsByName = TryLoadBeastDefinitions();
        var activeSkillNames = TryLoadNameSet(ActiveSkillsFileName);
        var passiveSkillNames = TryLoadNameSet(PassiveSkillsFileName);
        var beastSkillNames = TryLoadNameSet(BeastSkillsFileName);

        if (travelersByName is null
            || beastsByName is null
            || activeSkillNames is null
            || passiveSkillNames is null
            || beastSkillNames is null)
        {
            return null;
        }

        return new RuntimeDataCatalog(
            travelersByName,
            beastsByName,
            activeSkillNames,
            passiveSkillNames,
            beastSkillNames);
    }

    private Dictionary<string, TravelerDefinition>? TryLoadTravelerDefinitions()
        => TryLoadFromJsonFile(CharactersFileName, ParseTravelerDefinitions);

    private Dictionary<string, BeastDefinition>? TryLoadBeastDefinitions()
        => TryLoadFromJsonFile(EnemiesFileName, ParseBeastDefinitions);

    private IReadOnlySet<string>? TryLoadNameSet(string fileName)
        => TryLoadFromJsonFile(fileName, ParseNameSet);

    private TData? TryLoadFromJsonFile<TData>(string fileName, Func<JsonElement, TData?> parser)
        where TData : class
    {
        var jsonContent = TryReadJsonContent(fileName);
        if (jsonContent is null)
            return null;

        try
        {
            using var document = JsonDocument.Parse(jsonContent);
            return parser(document.RootElement);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private string? TryReadJsonContent(string fileName)
    {
        var filePath = Path.Combine(_dataFolderPath, fileName);
        if (!File.Exists(filePath))
            return null;

        try
        {
            return File.ReadAllText(filePath);
        }
        catch (IOException)
        {
            return null;
        }
        catch (UnauthorizedAccessException)
        {
            return null;
        }
    }

    private static Dictionary<string, TravelerDefinition>? ParseTravelerDefinitions(JsonElement rootElement)
    {
        if (rootElement.ValueKind != JsonValueKind.Array)
            return null;

        var travelersByName = new Dictionary<string, TravelerDefinition>(StringComparer.Ordinal);
        foreach (var travelerElement in rootElement.EnumerateArray())
        {
            var travelerDefinition = TryParseTravelerDefinition(travelerElement);
            if (travelerDefinition is null)
                return null;

            if (!travelersByName.TryAdd(travelerDefinition.Name, travelerDefinition))
                return null;
        }

        return travelersByName;
    }

    private static TravelerDefinition? TryParseTravelerDefinition(JsonElement travelerElement)
    {
        if (!TryGetRequiredString(travelerElement, NamePropertyName, out var travelerName)
            || !travelerElement.TryGetProperty(StatsPropertyName, out var statsElement)
            || !TryGetRequiredInt(statsElement, HpPropertyName, out var maxHp)
            || !TryGetRequiredInt(statsElement, SpPropertyName, out var maxSp)
            || !TryGetRequiredInt(statsElement, PhysAtkPropertyName, out var physAtk)
            || !TryGetRequiredInt(statsElement, PhysDefPropertyName, out var physDef)
            || !TryGetRequiredInt(statsElement, SpeedPropertyName, out var speed)
            || !TryReadStringList(travelerElement, WeaponsPropertyName, out var weapons))
        {
            return null;
        }

        return new TravelerDefinition(travelerName, maxHp, maxSp, physAtk, physDef, speed, weapons);
    }

    private static Dictionary<string, BeastDefinition>? ParseBeastDefinitions(JsonElement rootElement)
    {
        if (rootElement.ValueKind != JsonValueKind.Array)
            return null;

        var beastsByName = new Dictionary<string, BeastDefinition>(StringComparer.Ordinal);
        foreach (var beastElement in rootElement.EnumerateArray())
        {
            var beastDefinition = TryParseBeastDefinition(beastElement);
            if (beastDefinition is null)
                return null;

            if (!beastsByName.TryAdd(beastDefinition.Name, beastDefinition))
                return null;
        }

        return beastsByName;
    }

    private static BeastDefinition? TryParseBeastDefinition(JsonElement beastElement)
    {
        if (!TryGetRequiredString(beastElement, NamePropertyName, out var beastName)
            || !beastElement.TryGetProperty(StatsPropertyName, out var statsElement)
            || !TryGetRequiredInt(statsElement, HpPropertyName, out var maxHp)
            || !TryGetRequiredInt(statsElement, PhysAtkPropertyName, out var physAtk)
            || !TryGetRequiredInt(statsElement, PhysDefPropertyName, out var physDef)
            || !TryGetRequiredInt(statsElement, SpeedPropertyName, out var speed)
            || !TryGetRequiredInt(beastElement, ShieldsPropertyName, out var maxShields)
            || !TryGetRequiredString(beastElement, SkillPropertyName, out var skillName))
        {
            return null;
        }

        return new BeastDefinition(beastName, maxHp, physAtk, physDef, speed, maxShields, skillName);
    }

    private static IReadOnlySet<string>? ParseNameSet(JsonElement rootElement)
    {
        if (rootElement.ValueKind != JsonValueKind.Array)
            return null;

        var names = new HashSet<string>(StringComparer.Ordinal);
        foreach (var itemElement in rootElement.EnumerateArray())
        {
            if (!TryGetRequiredString(itemElement, NamePropertyName, out var name))
                return null;

            names.Add(name);
        }

        return names;
    }

    private static bool TryGetRequiredString(JsonElement sourceElement, string propertyName, out string propertyValue)
    {
        propertyValue = string.Empty;
        if (!sourceElement.TryGetProperty(propertyName, out var propertyElement))
            return false;

        var stringValue = propertyElement.GetString();
        if (string.IsNullOrWhiteSpace(stringValue))
            return false;

        propertyValue = stringValue;
        return true;
    }

    private static bool TryGetRequiredInt(JsonElement sourceElement, string propertyName, out int propertyValue)
    {
        propertyValue = 0;
        if (!sourceElement.TryGetProperty(propertyName, out var propertyElement))
            return false;

        return propertyElement.TryGetInt32(out propertyValue);
    }

    private static bool TryReadStringList(JsonElement sourceElement, string propertyName, out IReadOnlyList<string> values)
    {
        values = [];
        if (!sourceElement.TryGetProperty(propertyName, out var propertyElement)
            || propertyElement.ValueKind != JsonValueKind.Array)
        {
            return false;
        }

        var stringValues = new List<string>();
        foreach (var arrayElement in propertyElement.EnumerateArray())
        {
            var value = arrayElement.GetString();
            if (string.IsNullOrWhiteSpace(value))
                return false;

            stringValues.Add(value);
        }

        values = stringValues;
        return true;
    }
}

using System.Text.Json;

namespace Octopath_Traveler_Models.RuntimeData;

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

    public RuntimeDataCatalog Load()
    {
        var travelersByName = LoadTravelerDefinitions();
        var beastsByName = LoadBeastDefinitions();
        var activeSkillNames = LoadNameSet(ActiveSkillsFileName);
        var passiveSkillNames = LoadNameSet(PassiveSkillsFileName);
        var beastSkillNames = LoadNameSet(BeastSkillsFileName);

        return new RuntimeDataCatalog(
            travelersByName,
            beastsByName,
            activeSkillNames,
            passiveSkillNames,
            beastSkillNames);
    }

    private Dictionary<string, TravelerDefinition> LoadTravelerDefinitions()
        => LoadFromJsonFile(CharactersFileName, ParseTravelerDefinitions);

    private Dictionary<string, BeastDefinition> LoadBeastDefinitions()
        => LoadFromJsonFile(EnemiesFileName, ParseBeastDefinitions);

    private IReadOnlySet<string> LoadNameSet(string fileName)
        => LoadFromJsonFile(fileName, ParseNameSet);

    private TData LoadFromJsonFile<TData>(string fileName, Func<JsonElement, TData> parser)
        where TData : class
    {
        var jsonContent = ReadJsonContent(fileName);

        try
        {
            using var document = JsonDocument.Parse(jsonContent);
            return parser(document.RootElement);
        }
        catch (JsonException exception)
        {
            throw new RuntimeDataCatalogLoadException(
                $"Runtime data file '{fileName}' contains invalid JSON.",
                exception);
        }
    }

    private string ReadJsonContent(string fileName)
    {
        var filePath = Path.Combine(_dataFolderPath, fileName);
        if (!File.Exists(filePath))
            throw new RuntimeDataCatalogLoadException($"Runtime data file '{fileName}' was not found.");

        try
        {
            return File.ReadAllText(filePath);
        }
        catch (IOException exception)
        {
            throw new RuntimeDataCatalogLoadException(
                $"Runtime data file '{fileName}' could not be read.",
                exception);
        }
        catch (UnauthorizedAccessException exception)
        {
            throw new RuntimeDataCatalogLoadException(
                $"Access to runtime data file '{fileName}' was denied.",
                exception);
        }
    }

    private static Dictionary<string, TravelerDefinition> ParseTravelerDefinitions(JsonElement rootElement)
    {
        if (rootElement.ValueKind != JsonValueKind.Array)
            throw new RuntimeDataCatalogLoadException("Traveler definitions must be a JSON array.");

        var travelersByName = new Dictionary<string, TravelerDefinition>(StringComparer.Ordinal);
        foreach (var travelerElement in rootElement.EnumerateArray())
        {
            var travelerDefinition = ParseTravelerDefinition(travelerElement);

            if (!travelersByName.TryAdd(travelerDefinition.Name, travelerDefinition))
                throw new RuntimeDataCatalogLoadException($"Duplicate traveler definition '{travelerDefinition.Name}'.");
        }

        return travelersByName;
    }

    private static TravelerDefinition ParseTravelerDefinition(JsonElement travelerElement)
    {
        var travelerName = ReadRequiredString(travelerElement, NamePropertyName);
        var statsElement = ReadRequiredObject(travelerElement, StatsPropertyName);

        var maxHp = ReadRequiredInt(statsElement, HpPropertyName);
        var maxSp = ReadRequiredInt(statsElement, SpPropertyName);
        var physAtk = ReadRequiredInt(statsElement, PhysAtkPropertyName);
        var physDef = ReadRequiredInt(statsElement, PhysDefPropertyName);
        var speed = ReadRequiredInt(statsElement, SpeedPropertyName);
        var weapons = ReadStringList(travelerElement, WeaponsPropertyName);

        return new TravelerDefinition(travelerName, maxHp, maxSp, physAtk, physDef, speed, weapons);
    }

    private static Dictionary<string, BeastDefinition> ParseBeastDefinitions(JsonElement rootElement)
    {
        if (rootElement.ValueKind != JsonValueKind.Array)
            throw new RuntimeDataCatalogLoadException("Beast definitions must be a JSON array.");

        var beastsByName = new Dictionary<string, BeastDefinition>(StringComparer.Ordinal);
        foreach (var beastElement in rootElement.EnumerateArray())
        {
            var beastDefinition = ParseBeastDefinition(beastElement);

            if (!beastsByName.TryAdd(beastDefinition.Name, beastDefinition))
                throw new RuntimeDataCatalogLoadException($"Duplicate beast definition '{beastDefinition.Name}'.");
        }

        return beastsByName;
    }

    private static BeastDefinition ParseBeastDefinition(JsonElement beastElement)
    {
        var beastName = ReadRequiredString(beastElement, NamePropertyName);
        var statsElement = ReadRequiredObject(beastElement, StatsPropertyName);
        var maxShields = ReadRequiredInt(beastElement, ShieldsPropertyName);
        var skillName = ReadRequiredString(beastElement, SkillPropertyName);

        var maxHp = ReadRequiredInt(statsElement, HpPropertyName);
        var physAtk = ReadRequiredInt(statsElement, PhysAtkPropertyName);
        var physDef = ReadRequiredInt(statsElement, PhysDefPropertyName);
        var speed = ReadRequiredInt(statsElement, SpeedPropertyName);

        return new BeastDefinition(beastName, maxHp, physAtk, physDef, speed, maxShields, skillName);
    }

    private static IReadOnlySet<string> ParseNameSet(JsonElement rootElement)
    {
        if (rootElement.ValueKind != JsonValueKind.Array)
            throw new RuntimeDataCatalogLoadException("Skill catalogs must be a JSON array.");

        var names = new HashSet<string>(StringComparer.Ordinal);
        foreach (var itemElement in rootElement.EnumerateArray())
        {
            var name = ReadRequiredString(itemElement, NamePropertyName);
            names.Add(name);
        }

        return names;
    }

    private static string ReadRequiredString(JsonElement sourceElement, string propertyName)
    {
        if (!sourceElement.TryGetProperty(propertyName, out var propertyElement))
            throw new RuntimeDataCatalogLoadException($"Missing required property '{propertyName}'.");

        var stringValue = propertyElement.GetString();
        if (string.IsNullOrWhiteSpace(stringValue))
            throw new RuntimeDataCatalogLoadException($"Property '{propertyName}' cannot be empty.");

        return stringValue;
    }

    private static int ReadRequiredInt(JsonElement sourceElement, string propertyName)
    {
        if (!sourceElement.TryGetProperty(propertyName, out var propertyElement))
            throw new RuntimeDataCatalogLoadException($"Missing required property '{propertyName}'.");

        if (!propertyElement.TryGetInt32(out var value))
            throw new RuntimeDataCatalogLoadException($"Property '{propertyName}' must be an integer.");

        return value;
    }

    private static JsonElement ReadRequiredObject(JsonElement sourceElement, string propertyName)
    {
        if (!sourceElement.TryGetProperty(propertyName, out var propertyElement))
            throw new RuntimeDataCatalogLoadException($"Missing required object '{propertyName}'.");

        if (propertyElement.ValueKind != JsonValueKind.Object)
            throw new RuntimeDataCatalogLoadException($"Property '{propertyName}' must be an object.");

        return propertyElement;
    }

    private static IReadOnlyList<string> ReadStringList(JsonElement sourceElement, string propertyName)
    {
        if (!sourceElement.TryGetProperty(propertyName, out var propertyElement)
            || propertyElement.ValueKind != JsonValueKind.Array)
        {
            throw new RuntimeDataCatalogLoadException($"Property '{propertyName}' must be an array.");
        }

        return ReadNonEmptyStrings(propertyElement);
    }

    private static IReadOnlyList<string> ReadNonEmptyStrings(JsonElement arrayElement)
    {
        var stringValues = new List<string>();
        foreach (var itemElement in arrayElement.EnumerateArray())
        {
            var value = itemElement.GetString();
            if (string.IsNullOrWhiteSpace(value))
                throw new RuntimeDataCatalogLoadException("Array values cannot be empty.");

            stringValues.Add(value);
        }

        return stringValues;
    }
}


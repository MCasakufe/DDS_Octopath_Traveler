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
    private const string ElemAtkPropertyName = "ElemAtk";
    private const string ElemDefPropertyName = "ElemDef";
    private const string SpeedPropertyName = "Speed";
    private const string WeaponsPropertyName = "Weapons";
    private const string ShieldsPropertyName = "Shields";
    private const string SkillPropertyName = "Skill";
    private const string WeaknessesPropertyName = "Weaknesses";
    private const string TargetPropertyName = "Target";
    private const string TypePropertyName = "Type";
    private const string ModifierPropertyName = "Modifier";
    private const string SkillSpPropertyName = "SP";
    private const string DescriptionPropertyName = "Description";
    private const string BoostPropertyName = "Boost";
    private const string HitsPropertyName = "Hits";

    private readonly string _dataFolderPath;

    public RuntimeDataCatalogProvider(string teamsFolder)
    {
        _dataFolderPath = Path.GetDirectoryName(teamsFolder) ?? string.Empty;
    }

    public RuntimeDataCatalog Load()
    {
        Dictionary<string, TravelerDefinition> travelersByName = LoadTravelerDefinitions();
        Dictionary<string, BeastDefinition> beastsByName = LoadBeastDefinitions();
        Dictionary<string, SkillDefinition> activeSkillsByName = LoadSkillDefinitions();
        Dictionary<string, PassiveSkillDefinition> passiveSkillsByName = LoadPassiveSkillDefinitions();
        Dictionary<string, BeastSkillDefinition> beastSkillsByName = LoadBeastSkillDefinitions();
        IReadOnlySet<string> activeSkillNames = new HashSet<string>(activeSkillsByName.Keys, StringComparer.Ordinal);
        IReadOnlySet<string> passiveSkillNames = new HashSet<string>(passiveSkillsByName.Keys, StringComparer.Ordinal);
        IReadOnlySet<string> beastSkillNames = new HashSet<string>(beastSkillsByName.Keys, StringComparer.Ordinal);

        return new RuntimeDataCatalog(
            travelersByName,
            beastsByName,
            activeSkillsByName,
            beastSkillsByName,
            passiveSkillsByName,
            activeSkillNames,
            passiveSkillNames,
            beastSkillNames);
    }

    private Dictionary<string, TravelerDefinition> LoadTravelerDefinitions()
        => LoadFromJsonFile(CharactersFileName, ParseTravelerDefinitions);

    private Dictionary<string, BeastDefinition> LoadBeastDefinitions()
        => LoadFromJsonFile(EnemiesFileName, ParseBeastDefinitions);

    private Dictionary<string, SkillDefinition> LoadSkillDefinitions()
        => LoadFromJsonFile(ActiveSkillsFileName, ParseSkillDefinitions);

    private Dictionary<string, BeastSkillDefinition> LoadBeastSkillDefinitions()
        => LoadFromJsonFile(BeastSkillsFileName, ParseBeastSkillDefinitions);

    private Dictionary<string, PassiveSkillDefinition> LoadPassiveSkillDefinitions()
        => LoadFromJsonFile(PassiveSkillsFileName, ParsePassiveSkillDefinitions);

    private TData LoadFromJsonFile<TData>(string fileName, Func<JsonElement, TData> parser)
        where TData : class
    {
        string jsonContent = ReadJsonContent(fileName);

        try
        {
            using JsonDocument document = JsonDocument.Parse(jsonContent);
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
        string filePath = Path.Combine(_dataFolderPath, fileName);
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

        Dictionary<string, TravelerDefinition> travelersByName = new(StringComparer.Ordinal);
        foreach (JsonElement travelerElement in rootElement.EnumerateArray())
        {
            TravelerDefinition travelerDefinition = ParseTravelerDefinition(travelerElement);

            if (!travelersByName.TryAdd(travelerDefinition.Name, travelerDefinition))
                throw new RuntimeDataCatalogLoadException($"Duplicate traveler definition '{travelerDefinition.Name}'.");
        }

        return travelersByName;
    }

    private static TravelerDefinition ParseTravelerDefinition(JsonElement travelerElement)
    {
        string travelerName = ReadRequiredString(travelerElement, NamePropertyName);
        JsonElement statsElement = ReadRequiredObject(travelerElement, StatsPropertyName);

        int maxHp = ReadRequiredInt(statsElement, HpPropertyName);
        int maxSp = ReadRequiredInt(statsElement, SpPropertyName);
        int physAtk = ReadRequiredInt(statsElement, PhysAtkPropertyName);
        int physDef = ReadRequiredInt(statsElement, PhysDefPropertyName);
        int elemAtk = ReadRequiredInt(statsElement, ElemAtkPropertyName);
        int elemDef = ReadRequiredInt(statsElement, ElemDefPropertyName);
        int speed = ReadRequiredInt(statsElement, SpeedPropertyName);
        IReadOnlyList<string> weapons = ReadStringList(travelerElement, WeaponsPropertyName);

        return new TravelerDefinition(travelerName, maxHp, maxSp, physAtk, physDef, elemAtk, elemDef, speed, weapons);
    }

    private static Dictionary<string, BeastDefinition> ParseBeastDefinitions(JsonElement rootElement)
    {
        if (rootElement.ValueKind != JsonValueKind.Array)
            throw new RuntimeDataCatalogLoadException("Beast definitions must be a JSON array.");

        Dictionary<string, BeastDefinition> beastsByName = new(StringComparer.Ordinal);
        foreach (JsonElement beastElement in rootElement.EnumerateArray())
        {
            BeastDefinition beastDefinition = ParseBeastDefinition(beastElement);

            if (!beastsByName.TryAdd(beastDefinition.Name, beastDefinition))
                throw new RuntimeDataCatalogLoadException($"Duplicate beast definition '{beastDefinition.Name}'.");
        }

        return beastsByName;
    }

    private static BeastDefinition ParseBeastDefinition(JsonElement beastElement)
    {
        string beastName = ReadRequiredString(beastElement, NamePropertyName);
        JsonElement statsElement = ReadRequiredObject(beastElement, StatsPropertyName);
        int maxShields = ReadRequiredInt(beastElement, ShieldsPropertyName);
        string skillName = ReadRequiredString(beastElement, SkillPropertyName);

        int maxHp = ReadRequiredInt(statsElement, HpPropertyName);
        int physAtk = ReadRequiredInt(statsElement, PhysAtkPropertyName);
        int physDef = ReadRequiredInt(statsElement, PhysDefPropertyName);
        int elemAtk = ReadRequiredInt(statsElement, ElemAtkPropertyName);
        int elemDef = ReadRequiredInt(statsElement, ElemDefPropertyName);
        int speed = ReadRequiredInt(statsElement, SpeedPropertyName);
        IReadOnlySet<string> weaknesses = ReadStringSet(beastElement, WeaknessesPropertyName);

        return new BeastDefinition(beastName, maxHp, physAtk, physDef, elemAtk, elemDef, speed, maxShields, skillName, weaknesses);
    }

    private static Dictionary<string, SkillDefinition> ParseSkillDefinitions(JsonElement rootElement)
    {
        if (rootElement.ValueKind != JsonValueKind.Array)
            throw new RuntimeDataCatalogLoadException("Skill definitions must be a JSON array.");

        Dictionary<string, SkillDefinition> skillsByName = new(StringComparer.Ordinal);
        foreach (JsonElement skillElement in rootElement.EnumerateArray())
        {
            SkillDefinition skillDefinition = ParseSkillDefinition(skillElement);
            if (!skillsByName.TryAdd(skillDefinition.Name, skillDefinition))
                throw new RuntimeDataCatalogLoadException($"Duplicate skill definition '{skillDefinition.Name}'.");
        }

        return skillsByName;
    }

    private static SkillDefinition ParseSkillDefinition(JsonElement skillElement)
    {
        string skillName = ReadRequiredString(skillElement, NamePropertyName);
        int sp = ReadRequiredInt(skillElement, SkillSpPropertyName);
        string description = ReadRequiredString(skillElement, DescriptionPropertyName);
        string type = ReadOptionalString(skillElement, TypePropertyName);
        string target = ReadRequiredString(skillElement, TargetPropertyName);
        double modifier = ReadRequiredDouble(skillElement, ModifierPropertyName);
        string boost = ReadRequiredString(skillElement, BoostPropertyName);
        int hits = ReadOptionalInt(skillElement, HitsPropertyName, 1);

        return new SkillDefinition(skillName, sp, description, type, target, modifier, boost, hits);
    }

    private static Dictionary<string, BeastSkillDefinition> ParseBeastSkillDefinitions(JsonElement rootElement)
    {
        if (rootElement.ValueKind != JsonValueKind.Array)
            throw new RuntimeDataCatalogLoadException("Beast skill definitions must be a JSON array.");

        Dictionary<string, BeastSkillDefinition> skillsByName = new(StringComparer.Ordinal);
        foreach (JsonElement skillElement in rootElement.EnumerateArray())
        {
            BeastSkillDefinition skillDefinition = ParseBeastSkillDefinition(skillElement);
            if (!skillsByName.TryAdd(skillDefinition.Name, skillDefinition))
                throw new RuntimeDataCatalogLoadException($"Duplicate beast skill definition '{skillDefinition.Name}'.");
        }

        return skillsByName;
    }

    private static BeastSkillDefinition ParseBeastSkillDefinition(JsonElement skillElement)
    {
        string skillName = ReadRequiredString(skillElement, NamePropertyName);
        double modifier = ReadRequiredDouble(skillElement, ModifierPropertyName);
        string description = ReadRequiredString(skillElement, DescriptionPropertyName);
        string target = ReadRequiredString(skillElement, TargetPropertyName);
        int hits = ReadRequiredInt(skillElement, HitsPropertyName);

        return new BeastSkillDefinition(skillName, modifier, description, target, hits);
    }

    private static Dictionary<string, PassiveSkillDefinition> ParsePassiveSkillDefinitions(JsonElement rootElement)
    {
        if (rootElement.ValueKind != JsonValueKind.Array)
            throw new RuntimeDataCatalogLoadException("Passive skill definitions must be a JSON array.");

        Dictionary<string, PassiveSkillDefinition> skillsByName = new(StringComparer.Ordinal);
        foreach (JsonElement skillElement in rootElement.EnumerateArray())
        {
            PassiveSkillDefinition skillDefinition = ParsePassiveSkillDefinition(skillElement);
            if (!skillsByName.TryAdd(skillDefinition.Name, skillDefinition))
                throw new RuntimeDataCatalogLoadException($"Duplicate passive skill definition '{skillDefinition.Name}'.");
        }

        return skillsByName;
    }

    private static PassiveSkillDefinition ParsePassiveSkillDefinition(JsonElement skillElement)
    {
        string skillName = ReadRequiredString(skillElement, NamePropertyName);
        string description = ReadRequiredString(skillElement, DescriptionPropertyName);
        string target = ReadRequiredString(skillElement, TargetPropertyName);
        return new PassiveSkillDefinition(skillName, description, target);
    }

    private static string ReadRequiredString(JsonElement sourceElement, string propertyName)
    {
        if (!sourceElement.TryGetProperty(propertyName, out JsonElement propertyElement))
            throw new RuntimeDataCatalogLoadException($"Missing required property '{propertyName}'.");

        string? stringValue = propertyElement.GetString();
        if (string.IsNullOrWhiteSpace(stringValue))
            throw new RuntimeDataCatalogLoadException($"Property '{propertyName}' cannot be empty.");

        return stringValue;
    }

    private static int ReadRequiredInt(JsonElement sourceElement, string propertyName)
    {
        if (!sourceElement.TryGetProperty(propertyName, out JsonElement propertyElement))
            throw new RuntimeDataCatalogLoadException($"Missing required property '{propertyName}'.");

        if (!propertyElement.TryGetInt32(out int value))
            throw new RuntimeDataCatalogLoadException($"Property '{propertyName}' must be an integer.");

        return value;
    }

    private static int ReadOptionalInt(JsonElement sourceElement, string propertyName, int defaultValue)
    {
        if (!sourceElement.TryGetProperty(propertyName, out JsonElement propertyElement))
            return defaultValue;

        if (!propertyElement.TryGetInt32(out int value))
            throw new RuntimeDataCatalogLoadException($"Property '{propertyName}' must be an integer.");

        return value;
    }

    private static double ReadRequiredDouble(JsonElement sourceElement, string propertyName)
    {
        if (!sourceElement.TryGetProperty(propertyName, out JsonElement propertyElement))
            throw new RuntimeDataCatalogLoadException($"Missing required property '{propertyName}'.");

        if (!propertyElement.TryGetDouble(out double value))
            throw new RuntimeDataCatalogLoadException($"Property '{propertyName}' must be a number.");

        return value;
    }

    private static string ReadOptionalString(JsonElement sourceElement, string propertyName)
    {
        if (!sourceElement.TryGetProperty(propertyName, out JsonElement propertyElement))
            return string.Empty;

        string? stringValue = propertyElement.GetString();
        return stringValue?.Trim() ?? string.Empty;
    }

    private static JsonElement ReadRequiredObject(JsonElement sourceElement, string propertyName)
    {
        if (!sourceElement.TryGetProperty(propertyName, out JsonElement propertyElement))
            throw new RuntimeDataCatalogLoadException($"Missing required object '{propertyName}'.");

        if (propertyElement.ValueKind != JsonValueKind.Object)
            throw new RuntimeDataCatalogLoadException($"Property '{propertyName}' must be an object.");

        return propertyElement;
    }

    private static IReadOnlyList<string> ReadStringList(JsonElement sourceElement, string propertyName)
    {
        if (!sourceElement.TryGetProperty(propertyName, out JsonElement propertyElement)
            || propertyElement.ValueKind != JsonValueKind.Array)
        {
            throw new RuntimeDataCatalogLoadException($"Property '{propertyName}' must be an array.");
        }

        return ReadNonEmptyStrings(propertyElement);
    }

    private static IReadOnlySet<string> ReadStringSet(JsonElement sourceElement, string propertyName)
        => new HashSet<string>(ReadStringList(sourceElement, propertyName), StringComparer.Ordinal);

    private static IReadOnlyList<string> ReadNonEmptyStrings(JsonElement arrayElement)
    {
        List<string> stringValues = [];
        foreach (JsonElement itemElement in arrayElement.EnumerateArray())
        {
            string? value = itemElement.GetString();
            if (string.IsNullOrWhiteSpace(value))
                throw new RuntimeDataCatalogLoadException("Array values cannot be empty.");

            stringValues.Add(value);
        }

        return stringValues;
    }
}


using System.Text.Json;

namespace Octopath_Traveler.RuntimeData;

public sealed class RuntimeDataCatalogProvider
{
    private readonly string _dataFolderPath;

    public RuntimeDataCatalogProvider(string teamsFolder)
    {
        _dataFolderPath = Path.GetDirectoryName(teamsFolder) ?? string.Empty;
    }

    public RuntimeDataCatalog? TryLoad()
    {
        var travelersByName = TryLoadTravelerDefinitions();
        var beastsByName = TryLoadBeastDefinitions();
        var activeSkillNames = TryLoadNameSet("skills.json");
        var passiveSkillNames = TryLoadNameSet("passive_skills.json");
        var beastSkillNames = TryLoadNameSet("beast_skills.json");

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
    {
        var jsonContent = TryReadJsonContent("characters.json");
        if (jsonContent is null)
            return null;

        try
        {
            using var document = JsonDocument.Parse(jsonContent);
            return ParseTravelerDefinitions(document.RootElement);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private Dictionary<string, BeastDefinition>? TryLoadBeastDefinitions()
    {
        var jsonContent = TryReadJsonContent("enemies.json");
        if (jsonContent is null)
            return null;

        try
        {
            using var document = JsonDocument.Parse(jsonContent);
            return ParseBeastDefinitions(document.RootElement);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private HashSet<string>? TryLoadNameSet(string fileName)
    {
        var jsonContent = TryReadJsonContent(fileName);
        if (jsonContent is null)
            return null;

        try
        {
            using var document = JsonDocument.Parse(jsonContent);
            return ParseNameSet(document.RootElement);
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
        if (!TryGetRequiredString(travelerElement, "Name", out var travelerName)
            || !travelerElement.TryGetProperty("Stats", out var statsElement)
            || !TryGetRequiredInt(statsElement, "HP", out var maxHp)
            || !TryGetRequiredInt(statsElement, "SP", out var maxSp)
            || !TryGetRequiredInt(statsElement, "PhysAtk", out var physAtk)
            || !TryGetRequiredInt(statsElement, "PhysDef", out var physDef)
            || !TryGetRequiredInt(statsElement, "Speed", out var speed)
            || !TryReadStringList(travelerElement, "Weapons", out var weapons))
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
        if (!TryGetRequiredString(beastElement, "Name", out var beastName)
            || !beastElement.TryGetProperty("Stats", out var statsElement)
            || !TryGetRequiredInt(statsElement, "HP", out var maxHp)
            || !TryGetRequiredInt(statsElement, "PhysAtk", out var physAtk)
            || !TryGetRequiredInt(statsElement, "PhysDef", out var physDef)
            || !TryGetRequiredInt(statsElement, "Speed", out var speed)
            || !TryGetRequiredInt(beastElement, "Shields", out var maxShields)
            || !TryGetRequiredString(beastElement, "Skill", out var skillName))
        {
            return null;
        }

        return new BeastDefinition(beastName, maxHp, physAtk, physDef, speed, maxShields, skillName);
    }

    private static HashSet<string>? ParseNameSet(JsonElement rootElement)
    {
        if (rootElement.ValueKind != JsonValueKind.Array)
            return null;

        var names = new HashSet<string>(StringComparer.Ordinal);
        foreach (var itemElement in rootElement.EnumerateArray())
        {
            if (!TryGetRequiredString(itemElement, "Name", out var name))
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
using System.Text.Json;

namespace Octopath_Traveler_Models.TeamSelection;

public sealed class JsonValidationCatalogProvider
{
    private const string NamePropertyName = "Name";

    private const string CharactersFileName = "characters.json";
    private const string EnemiesFileName = "enemies.json";
    private const string SkillsFileName = "skills.json";
    private const string PassiveSkillsFileName = "passive_skills.json";

    private readonly string _dataFolderPath;

    public JsonValidationCatalogProvider(string teamsFolder)
    {
        _dataFolderPath = Path.GetDirectoryName(teamsFolder) ?? string.Empty;
    }

    public ValidationCatalog Load()
    {
        IReadOnlySet<string> validTravelerNames = LoadNameSet(CharactersFileName);
        IReadOnlySet<string> validBeastNames = LoadNameSet(EnemiesFileName);
        IReadOnlySet<string> validActiveSkillNames = LoadNameSet(SkillsFileName);
        IReadOnlySet<string> validPassiveSkillNames = LoadNameSet(PassiveSkillsFileName);

        return new ValidationCatalog(validTravelerNames, validBeastNames, validActiveSkillNames, validPassiveSkillNames);
    }

    private IReadOnlySet<string> LoadNameSet(string fileName)
    {
        string fullPath = Path.Combine(_dataFolderPath, fileName);
        string json = ReadJson(fullPath, fileName);
        return ParseNameSet(json, fileName);
    }

    private static string ReadJson(string fullPath, string fileName)
    {
        if (!File.Exists(fullPath))
            throw new ValidationCatalogLoadException($"Validation catalog file '{fileName}' was not found.");

        try
        {
            return File.ReadAllText(fullPath);
        }
        catch (IOException exception)
        {
            throw new ValidationCatalogLoadException(
                $"Validation catalog file '{fileName}' could not be read.",
                exception);
        }
        catch (UnauthorizedAccessException exception)
        {
            throw new ValidationCatalogLoadException(
                $"Access to validation catalog file '{fileName}' was denied.",
                exception);
        }
    }

    private static IReadOnlySet<string> ParseNameSet(string json, string fileName)
    {
        try
        {
            using JsonDocument document = JsonDocument.Parse(json);
            return ReadNames(document.RootElement, fileName);
        }
        catch (JsonException exception)
        {
            throw new ValidationCatalogLoadException(
                $"Validation catalog file '{fileName}' contains invalid JSON.",
                exception);
        }
    }

    private static IReadOnlySet<string> ReadNames(JsonElement rootElement, string fileName)
    {
        if (rootElement.ValueKind != JsonValueKind.Array)
            throw new ValidationCatalogLoadException($"Validation catalog file '{fileName}' must contain a JSON array.");

        HashSet<string> names = new(StringComparer.Ordinal);
        foreach (JsonElement item in rootElement.EnumerateArray())
        {
            if (!item.TryGetProperty(NamePropertyName, out JsonElement nameElement))
                throw new ValidationCatalogLoadException($"Validation catalog file '{fileName}' contains an entry without '{NamePropertyName}'.");

            string? name = nameElement.GetString();
            if (string.IsNullOrWhiteSpace(name))
                throw new ValidationCatalogLoadException($"Validation catalog file '{fileName}' contains an empty '{NamePropertyName}'.");

            names.Add(name);
        }

        return names;
    }
}


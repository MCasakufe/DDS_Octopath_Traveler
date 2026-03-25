using System.Text.Json;

namespace Octopath_Traveler.TeamSelection;

public sealed class JsonValidationCatalogProvider
{
    private const string CharactersFileName = "characters.json";
    private const string EnemiesFileName = "enemies.json";
    private const string SkillsFileName = "skills.json";
    private const string PassiveSkillsFileName = "passive_skills.json";

    private readonly string _dataFolderPath;

    public JsonValidationCatalogProvider(string teamsFolder)
    {
        _dataFolderPath = Path.GetDirectoryName(teamsFolder) ?? string.Empty;
    }

    public ValidationCatalog? TryLoad()
    {
        var validTravelerNames = TryLoadNameSet(CharactersFileName);
        var validBeastNames = TryLoadNameSet(EnemiesFileName);
        var validActiveSkillNames = TryLoadNameSet(SkillsFileName);
        var validPassiveSkillNames = TryLoadNameSet(PassiveSkillsFileName);

        if (validTravelerNames is null
            || validBeastNames is null
            || validActiveSkillNames is null
            || validPassiveSkillNames is null)
        {
            return null;
        }

        return new ValidationCatalog(validTravelerNames, validBeastNames, validActiveSkillNames, validPassiveSkillNames);
    }

    private IReadOnlySet<string>? TryLoadNameSet(string fileName)
    {
        var fullPath = Path.Combine(_dataFolderPath, fileName);
        if (!File.Exists(fullPath))
            return null;

        try
        {
            var json = File.ReadAllText(fullPath);
            using var document = JsonDocument.Parse(json);
            return TryReadNames(document.RootElement);
        }
        catch (IOException)
        {
            return null;
        }
        catch (UnauthorizedAccessException)
        {
            return null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static IReadOnlySet<string>? TryReadNames(JsonElement rootElement)
    {
        if (rootElement.ValueKind != JsonValueKind.Array)
            return null;

        var names = new HashSet<string>(StringComparer.Ordinal);
        foreach (var item in rootElement.EnumerateArray())
        {
            if (!item.TryGetProperty("Name", out var nameElement))
                continue;

            var name = nameElement.GetString();
            if (!string.IsNullOrWhiteSpace(name))
                names.Add(name);
        }

        return names;
    }
}

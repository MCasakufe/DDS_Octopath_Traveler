using System.Text.Json;

namespace Octopath_Traveler.TeamSelection;

public sealed class JsonValidationCatalogProvider
{
    private readonly string _dataFolderPath;

    public JsonValidationCatalogProvider(string teamsFolder)
    {
        _dataFolderPath = Path.GetDirectoryName(teamsFolder) ?? string.Empty;
    }

    public ValidationCatalog? TryLoad()
    {
        var validTravelerNames = TryLoadNameSet("characters.json");
        var validBeastNames = TryLoadNameSet("enemies.json");
        var validActiveSkillNames = TryLoadNameSet("skills.json");
        var validPassiveSkillNames = TryLoadNameSet("passive_skills.json");

        if (validTravelerNames is null
            || validBeastNames is null
            || validActiveSkillNames is null
            || validPassiveSkillNames is null)
        {
            return null;
        }

        return new ValidationCatalog(validTravelerNames, validBeastNames, validActiveSkillNames, validPassiveSkillNames);
    }

    private HashSet<string>? TryLoadNameSet(string fileName)
    {
        var fullPath = Path.Combine(_dataFolderPath, fileName);
        if (!File.Exists(fullPath))
            return null;

        try
        {
            var json = File.ReadAllText(fullPath);
            using var document = JsonDocument.Parse(json);
            return ReadNames(document.RootElement);
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

    private static HashSet<string> ReadNames(JsonElement rootElement)
    {
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

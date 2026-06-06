using System.Text.Json;

namespace Octopath_Traveler_Models.TeamSelection;

internal sealed class JsonValidationCatalogParser
{
    private const string NamePropertyName = "Name";

    public IReadOnlySet<string> ParseEntityNames(string jsonContent, string fileName)
    {
        try
        {
            using JsonDocument document = JsonDocument.Parse(jsonContent);
            return ReadEntityNames(document.RootElement, fileName);
        }
        catch (JsonException exception)
        {
            throw new ValidationCatalogLoadException(
                $"Validation catalog file '{fileName}' contains invalid JSON.",
                exception);
        }
    }

    private static IReadOnlySet<string> ReadEntityNames(JsonElement rootElement, string fileName)
    {
        EnsureRootElementIsArray(rootElement, fileName);

        HashSet<string> names = new(StringComparer.Ordinal);
        foreach (JsonElement itemElement in rootElement.EnumerateArray())
            names.Add(ReadEntityName(itemElement, fileName));

        return names;
    }

    private static void EnsureRootElementIsArray(JsonElement rootElement, string fileName)
    {
        if (rootElement.ValueKind != JsonValueKind.Array)
            throw new ValidationCatalogLoadException(
                $"Validation catalog file '{fileName}' must contain a JSON array.");
    }

    private static string ReadEntityName(JsonElement itemElement, string fileName)
    {
        JsonElement nameElement = ReadNameElement(itemElement, fileName);
        string? name = nameElement.GetString();
        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationCatalogLoadException(
                $"Validation catalog file '{fileName}' contains an empty '{NamePropertyName}'.");

        return name;
    }

    private static JsonElement ReadNameElement(JsonElement itemElement, string fileName)
    {
        if (!itemElement.TryGetProperty(NamePropertyName, out JsonElement nameElement))
        {
            throw new ValidationCatalogLoadException(
                $"Validation catalog file '{fileName}' contains an entry without '{NamePropertyName}'.");
        }

        return nameElement;
    }
}

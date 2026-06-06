using System.Text.Json;

namespace Octopath_Traveler_Models.RuntimeData;

internal static class RuntimeDataDefinitionDictionaryParser
{
    public static Dictionary<string, TDefinition> ParseDefinitions<TDefinition>(
        JsonElement rootElement,
        string collectionDescription,
        string duplicateDescription,
        Func<JsonElement, TDefinition> parseDefinition,
        Func<TDefinition, string> selectDefinitionName)
    {
        EnsureRootElementIsArray(rootElement, collectionDescription);

        Dictionary<string, TDefinition> definitionsByName = new(StringComparer.Ordinal);
        foreach (JsonElement definitionElement in rootElement.EnumerateArray())
        {
            AddDefinition(
                definitionsByName,
                definitionElement,
                duplicateDescription,
                parseDefinition,
                selectDefinitionName);
        }

        return definitionsByName;
    }

    private static void EnsureRootElementIsArray(JsonElement rootElement, string collectionDescription)
    {
        if (rootElement.ValueKind != JsonValueKind.Array)
            throw new RuntimeDataCatalogLoadException($"{collectionDescription} must be a JSON array.");
    }

    private static void AddDefinition<TDefinition>(
        Dictionary<string, TDefinition> definitionsByName,
        JsonElement definitionElement,
        string duplicateDescription,
        Func<JsonElement, TDefinition> parseDefinition,
        Func<TDefinition, string> selectDefinitionName)
    {
        TDefinition definition = parseDefinition(definitionElement);
        string definitionName = selectDefinitionName(definition);
        if (!definitionsByName.TryAdd(definitionName, definition))
            throw new RuntimeDataCatalogLoadException($"Duplicate {duplicateDescription} '{definitionName}'.");
    }
}

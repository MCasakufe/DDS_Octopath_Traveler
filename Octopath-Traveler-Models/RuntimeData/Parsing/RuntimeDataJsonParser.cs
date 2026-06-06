using System.Text.Json;

namespace Octopath_Traveler_Models.RuntimeData;

internal static class RuntimeDataJsonParser
{
    public static TData Parse<TData>(
        string fileName,
        string jsonContent,
        Func<JsonElement, TData> parseRootElement)
        where TData : class
    {
        try
        {
            using JsonDocument document = JsonDocument.Parse(jsonContent);
            return parseRootElement(document.RootElement);
        }
        catch (JsonException exception)
        {
            throw new RuntimeDataCatalogLoadException(
                $"Runtime data file '{fileName}' contains invalid JSON.",
                exception);
        }
    }
}

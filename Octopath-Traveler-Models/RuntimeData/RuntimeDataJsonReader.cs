using System.Text.Json;

namespace Octopath_Traveler_Models.RuntimeData;

internal static class RuntimeDataJsonReader
{
    public static string ReadRequiredString(JsonElement sourceElement, string propertyName)
    {
        if (!sourceElement.TryGetProperty(propertyName, out JsonElement propertyElement))
            throw new RuntimeDataCatalogLoadException($"Missing required property '{propertyName}'.");

        string? stringValue = propertyElement.GetString();
        if (string.IsNullOrWhiteSpace(stringValue))
            throw new RuntimeDataCatalogLoadException($"Property '{propertyName}' cannot be empty.");

        return stringValue;
    }

    public static int ReadRequiredInt(JsonElement sourceElement, string propertyName)
    {
        if (!sourceElement.TryGetProperty(propertyName, out JsonElement propertyElement))
            throw new RuntimeDataCatalogLoadException($"Missing required property '{propertyName}'.");

        if (!propertyElement.TryGetInt32(out int value))
            throw new RuntimeDataCatalogLoadException($"Property '{propertyName}' must be an integer.");

        return value;
    }

    public static int ReadOptionalInt(JsonElement sourceElement, string propertyName, int defaultValue)
    {
        if (!sourceElement.TryGetProperty(propertyName, out JsonElement propertyElement))
            return defaultValue;

        if (!propertyElement.TryGetInt32(out int value))
            throw new RuntimeDataCatalogLoadException($"Property '{propertyName}' must be an integer.");

        return value;
    }

    public static double ReadRequiredDouble(JsonElement sourceElement, string propertyName)
    {
        if (!sourceElement.TryGetProperty(propertyName, out JsonElement propertyElement))
            throw new RuntimeDataCatalogLoadException($"Missing required property '{propertyName}'.");

        if (!propertyElement.TryGetDouble(out double value))
            throw new RuntimeDataCatalogLoadException($"Property '{propertyName}' must be a number.");

        return value;
    }

    public static string ReadOptionalString(JsonElement sourceElement, string propertyName)
    {
        if (!sourceElement.TryGetProperty(propertyName, out JsonElement propertyElement))
            return string.Empty;

        string? stringValue = propertyElement.GetString();
        return stringValue?.Trim() ?? string.Empty;
    }

    public static JsonElement ReadRequiredObject(JsonElement sourceElement, string propertyName)
    {
        if (!sourceElement.TryGetProperty(propertyName, out JsonElement propertyElement))
            throw new RuntimeDataCatalogLoadException($"Missing required object '{propertyName}'.");

        if (propertyElement.ValueKind != JsonValueKind.Object)
            throw new RuntimeDataCatalogLoadException($"Property '{propertyName}' must be an object.");

        return propertyElement;
    }

    public static IReadOnlyList<string> ReadStringList(JsonElement sourceElement, string propertyName)
    {
        if (!sourceElement.TryGetProperty(propertyName, out JsonElement propertyElement)
            || propertyElement.ValueKind != JsonValueKind.Array)
        {
            throw new RuntimeDataCatalogLoadException($"Property '{propertyName}' must be an array.");
        }

        return ReadNonEmptyStrings(propertyElement);
    }

    public static IReadOnlySet<string> ReadStringSet(JsonElement sourceElement, string propertyName)
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

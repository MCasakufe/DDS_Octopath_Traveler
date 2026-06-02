using System.Text.Json;

namespace Octopath_Traveler_Models.RuntimeData;

internal sealed class TravelerDefinitionParser
{
    public Dictionary<string, TravelerDefinition> Parse(string fileName, string jsonContent)
        => RuntimeDataJsonParser.Parse(fileName, jsonContent, ParseRootElement);

    private static Dictionary<string, TravelerDefinition> ParseRootElement(JsonElement rootElement)
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
        string travelerName = RuntimeDataJsonReader.ReadRequiredString(travelerElement, RuntimeDataPropertyNames.Name);
        JsonElement statsElement = RuntimeDataJsonReader.ReadRequiredObject(travelerElement, RuntimeDataPropertyNames.Stats);

        int maxHp = RuntimeDataJsonReader.ReadRequiredInt(statsElement, RuntimeDataPropertyNames.Hp);
        int maxSp = RuntimeDataJsonReader.ReadRequiredInt(statsElement, RuntimeDataPropertyNames.Sp);
        int physAtk = RuntimeDataJsonReader.ReadRequiredInt(statsElement, RuntimeDataPropertyNames.PhysAtk);
        int physDef = RuntimeDataJsonReader.ReadRequiredInt(statsElement, RuntimeDataPropertyNames.PhysDef);
        int elemAtk = RuntimeDataJsonReader.ReadRequiredInt(statsElement, RuntimeDataPropertyNames.ElemAtk);
        int elemDef = RuntimeDataJsonReader.ReadRequiredInt(statsElement, RuntimeDataPropertyNames.ElemDef);
        int speed = RuntimeDataJsonReader.ReadRequiredInt(statsElement, RuntimeDataPropertyNames.Speed);
        IReadOnlyList<string> weapons = RuntimeDataJsonReader.ReadStringList(
            travelerElement,
            RuntimeDataPropertyNames.Weapons);

        return new TravelerDefinition(travelerName, maxHp, maxSp, physAtk, physDef, elemAtk, elemDef, speed, weapons);
    }
}

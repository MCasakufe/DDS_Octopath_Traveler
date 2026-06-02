using System.Text.Json;

namespace Octopath_Traveler_Models.RuntimeData;

internal sealed class BeastDefinitionParser
{
    public Dictionary<string, BeastDefinition> Parse(string fileName, string jsonContent)
        => RuntimeDataJsonParser.Parse(fileName, jsonContent, ParseRootElement);

    private static Dictionary<string, BeastDefinition> ParseRootElement(JsonElement rootElement)
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
        string beastName = RuntimeDataJsonReader.ReadRequiredString(beastElement, RuntimeDataPropertyNames.Name);
        JsonElement statsElement = RuntimeDataJsonReader.ReadRequiredObject(beastElement, RuntimeDataPropertyNames.Stats);
        int maxShields = RuntimeDataJsonReader.ReadRequiredInt(beastElement, RuntimeDataPropertyNames.Shields);
        string skillName = RuntimeDataJsonReader.ReadRequiredString(beastElement, RuntimeDataPropertyNames.Skill);

        int maxHp = RuntimeDataJsonReader.ReadRequiredInt(statsElement, RuntimeDataPropertyNames.Hp);
        int physAtk = RuntimeDataJsonReader.ReadRequiredInt(statsElement, RuntimeDataPropertyNames.PhysAtk);
        int physDef = RuntimeDataJsonReader.ReadRequiredInt(statsElement, RuntimeDataPropertyNames.PhysDef);
        int elemAtk = RuntimeDataJsonReader.ReadRequiredInt(statsElement, RuntimeDataPropertyNames.ElemAtk);
        int elemDef = RuntimeDataJsonReader.ReadRequiredInt(statsElement, RuntimeDataPropertyNames.ElemDef);
        int speed = RuntimeDataJsonReader.ReadRequiredInt(statsElement, RuntimeDataPropertyNames.Speed);
        IReadOnlySet<string> weaknesses = RuntimeDataJsonReader.ReadStringSet(
            beastElement,
            RuntimeDataPropertyNames.Weaknesses);

        return new BeastDefinition(beastName, maxHp, physAtk, physDef, elemAtk, elemDef, speed, maxShields, skillName, weaknesses);
    }
}

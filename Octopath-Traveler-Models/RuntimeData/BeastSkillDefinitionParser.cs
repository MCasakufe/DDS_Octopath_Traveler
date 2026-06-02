using System.Text.Json;

namespace Octopath_Traveler_Models.RuntimeData;

internal sealed class BeastSkillDefinitionParser
{
    public Dictionary<string, BeastSkillDefinition> Parse(string fileName, string jsonContent)
        => RuntimeDataJsonParser.Parse(fileName, jsonContent, ParseRootElement);

    private static Dictionary<string, BeastSkillDefinition> ParseRootElement(JsonElement rootElement)
    {
        if (rootElement.ValueKind != JsonValueKind.Array)
            throw new RuntimeDataCatalogLoadException("Beast skill definitions must be a JSON array.");

        Dictionary<string, BeastSkillDefinition> skillsByName = new(StringComparer.Ordinal);
        foreach (JsonElement skillElement in rootElement.EnumerateArray())
        {
            BeastSkillDefinition skillDefinition = ParseBeastSkillDefinition(skillElement);
            if (!skillsByName.TryAdd(skillDefinition.Name, skillDefinition))
                throw new RuntimeDataCatalogLoadException($"Duplicate beast skill definition '{skillDefinition.Name}'.");
        }

        return skillsByName;
    }

    private static BeastSkillDefinition ParseBeastSkillDefinition(JsonElement skillElement)
    {
        string skillName = RuntimeDataJsonReader.ReadRequiredString(skillElement, RuntimeDataPropertyNames.Name);
        double modifier = RuntimeDataJsonReader.ReadRequiredDouble(skillElement, RuntimeDataPropertyNames.Modifier);
        string description = RuntimeDataJsonReader.ReadRequiredString(skillElement, RuntimeDataPropertyNames.Description);
        string target = RuntimeDataJsonReader.ReadRequiredString(skillElement, RuntimeDataPropertyNames.Target);
        int hits = RuntimeDataJsonReader.ReadRequiredInt(skillElement, RuntimeDataPropertyNames.Hits);

        return new BeastSkillDefinition(skillName, modifier, description, target, hits);
    }
}

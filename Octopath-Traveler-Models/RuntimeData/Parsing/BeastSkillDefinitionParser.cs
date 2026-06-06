using System.Text.Json;

namespace Octopath_Traveler_Models.RuntimeData;

internal sealed class BeastSkillDefinitionParser
{
    public Dictionary<string, BeastSkillDefinition> Parse(string fileName, string jsonContent)
        => RuntimeDataJsonParser.Parse(fileName, jsonContent, ParseRootElement);

    private static Dictionary<string, BeastSkillDefinition> ParseRootElement(JsonElement rootElement)
        => RuntimeDataDefinitionDictionaryParser.ParseDefinitions(
            rootElement,
            "Beast skill definitions",
            "beast skill definition",
            ParseBeastSkillDefinition,
            skillDefinition => skillDefinition.Name);

    private static BeastSkillDefinition ParseBeastSkillDefinition(JsonElement skillElement)
    {
        string skillName = RuntimeDataJsonReader.ReadRequiredString(skillElement, RuntimeDataPropertyNames.Name);
        double modifier = RuntimeDataJsonReader.ReadRequiredDouble(skillElement, RuntimeDataPropertyNames.Modifier);
        string description = RuntimeDataJsonReader.ReadRequiredString(
            skillElement,
            RuntimeDataPropertyNames.Description);
        string target = RuntimeDataJsonReader.ReadRequiredString(skillElement, RuntimeDataPropertyNames.Target);
        int hits = RuntimeDataJsonReader.ReadRequiredInt(skillElement, RuntimeDataPropertyNames.Hits);

        return new BeastSkillDefinition(skillName, modifier, description, target, hits);
    }
}

using System.Text.Json;

namespace Octopath_Traveler_Models.RuntimeData;

internal sealed class SkillDefinitionParser
{
    private const int DefaultSkillHitCount = 1;

    public Dictionary<string, SkillDefinition> Parse(string fileName, string jsonContent)
        => RuntimeDataJsonParser.Parse(fileName, jsonContent, ParseRootElement);

    private static Dictionary<string, SkillDefinition> ParseRootElement(JsonElement rootElement)
        => RuntimeDataDefinitionDictionaryParser.ParseDefinitions(
            rootElement,
            "Skill definitions",
            "skill definition",
            ParseSkillDefinition,
            skillDefinition => skillDefinition.Name);

    private static SkillDefinition ParseSkillDefinition(JsonElement skillElement)
    {
        string skillName = RuntimeDataJsonReader.ReadRequiredString(skillElement, RuntimeDataPropertyNames.Name);
        int sp = RuntimeDataJsonReader.ReadRequiredInt(skillElement, RuntimeDataPropertyNames.Sp);
        string description = RuntimeDataJsonReader.ReadRequiredString(
            skillElement,
            RuntimeDataPropertyNames.Description);
        string type = RuntimeDataJsonReader.ReadOptionalString(skillElement, RuntimeDataPropertyNames.Type);
        string target = RuntimeDataJsonReader.ReadRequiredString(skillElement, RuntimeDataPropertyNames.Target);
        double modifier = RuntimeDataJsonReader.ReadRequiredDouble(skillElement, RuntimeDataPropertyNames.Modifier);
        string boost = RuntimeDataJsonReader.ReadRequiredString(skillElement, RuntimeDataPropertyNames.Boost);
        int hits = RuntimeDataJsonReader.ReadOptionalInt(
            skillElement,
            RuntimeDataPropertyNames.Hits,
            DefaultSkillHitCount);

        return new SkillDefinition(skillName, sp, description, type, target, modifier, boost, hits);
    }
}

using System.Text.Json;

namespace Octopath_Traveler_Models.RuntimeData;

internal sealed class PassiveSkillDefinitionParser
{
    public Dictionary<string, PassiveSkillDefinition> Parse(string fileName, string jsonContent)
        => RuntimeDataJsonParser.Parse(fileName, jsonContent, ParseRootElement);

    private static Dictionary<string, PassiveSkillDefinition> ParseRootElement(JsonElement rootElement)
        => RuntimeDataDefinitionDictionaryParser.ParseDefinitions(
            rootElement,
            "Passive skill definitions",
            "passive skill definition",
            ParsePassiveSkillDefinition,
            skillDefinition => skillDefinition.Name);

    private static PassiveSkillDefinition ParsePassiveSkillDefinition(JsonElement skillElement)
    {
        string skillName = RuntimeDataJsonReader.ReadRequiredString(skillElement, RuntimeDataPropertyNames.Name);
        string description = RuntimeDataJsonReader.ReadRequiredString(
            skillElement,
            RuntimeDataPropertyNames.Description);
        string target = RuntimeDataJsonReader.ReadRequiredString(skillElement, RuntimeDataPropertyNames.Target);
        return new PassiveSkillDefinition(skillName, description, target);
    }
}

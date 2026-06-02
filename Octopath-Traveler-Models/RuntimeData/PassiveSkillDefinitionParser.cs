using System.Text.Json;

namespace Octopath_Traveler_Models.RuntimeData;

internal sealed class PassiveSkillDefinitionParser
{
    public Dictionary<string, PassiveSkillDefinition> Parse(string fileName, string jsonContent)
        => RuntimeDataJsonParser.Parse(fileName, jsonContent, ParseRootElement);

    private static Dictionary<string, PassiveSkillDefinition> ParseRootElement(JsonElement rootElement)
    {
        if (rootElement.ValueKind != JsonValueKind.Array)
            throw new RuntimeDataCatalogLoadException("Passive skill definitions must be a JSON array.");

        Dictionary<string, PassiveSkillDefinition> skillsByName = new(StringComparer.Ordinal);
        foreach (JsonElement skillElement in rootElement.EnumerateArray())
        {
            PassiveSkillDefinition skillDefinition = ParsePassiveSkillDefinition(skillElement);
            if (!skillsByName.TryAdd(skillDefinition.Name, skillDefinition))
                throw new RuntimeDataCatalogLoadException($"Duplicate passive skill definition '{skillDefinition.Name}'.");
        }

        return skillsByName;
    }

    private static PassiveSkillDefinition ParsePassiveSkillDefinition(JsonElement skillElement)
    {
        string skillName = RuntimeDataJsonReader.ReadRequiredString(skillElement, RuntimeDataPropertyNames.Name);
        string description = RuntimeDataJsonReader.ReadRequiredString(skillElement, RuntimeDataPropertyNames.Description);
        string target = RuntimeDataJsonReader.ReadRequiredString(skillElement, RuntimeDataPropertyNames.Target);
        return new PassiveSkillDefinition(skillName, description, target);
    }
}

namespace Octopath_Traveler_Models.RuntimeData;

internal sealed record RuntimeDataDefinitions(
    Dictionary<string, TravelerDefinition> TravelersByName,
    Dictionary<string, BeastDefinition> BeastsByName,
    Dictionary<string, SkillDefinition> ActiveSkillsByName,
    Dictionary<string, BeastSkillDefinition> BeastSkillsByName,
    Dictionary<string, PassiveSkillDefinition> PassiveSkillsByName);

namespace Octopath_Traveler_Models.RuntimeData;

public sealed record RuntimeDataCatalog(
    IReadOnlyDictionary<string, TravelerDefinition> TravelersByName,
    IReadOnlyDictionary<string, BeastDefinition> BeastsByName,
    IReadOnlyDictionary<string, SkillDefinition> ActiveSkillsByName,
    IReadOnlyDictionary<string, BeastSkillDefinition> BeastSkillsByName,
    IReadOnlyDictionary<string, PassiveSkillDefinition> PassiveSkillsByName,
    IReadOnlySet<string> ActiveSkillNames,
    IReadOnlySet<string> PassiveSkillNames,
    IReadOnlySet<string> BeastSkillNames)
{
    public SkillDefinition? SelectActiveSkillOrNull(string skillName)
        => ActiveSkillsByName.TryGetValue(skillName, out SkillDefinition? skillDefinition)
            ? skillDefinition
            : null;

    public BeastSkillDefinition? SelectBeastSkillOrNull(string skillName)
        => BeastSkillsByName.TryGetValue(skillName, out BeastSkillDefinition? skillDefinition)
            ? skillDefinition
            : null;

    public PassiveSkillDefinition? SelectPassiveSkillOrNull(string skillName)
        => PassiveSkillsByName.TryGetValue(skillName, out PassiveSkillDefinition? skillDefinition)
            ? skillDefinition
            : null;
}

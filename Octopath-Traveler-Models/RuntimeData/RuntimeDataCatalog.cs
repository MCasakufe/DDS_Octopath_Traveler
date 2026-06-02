namespace Octopath_Traveler_Models.RuntimeData;

public sealed record TravelerDefinition(
    string Name,
    int MaxHp,
    int MaxSp,
    int PhysAtk,
    int PhysDef,
    int ElemAtk,
    int ElemDef,
    int Speed,
    IReadOnlyList<string> Weapons);

public sealed record BeastDefinition(
    string Name,
    int MaxHp,
    int PhysAtk,
    int PhysDef,
    int ElemAtk,
    int ElemDef,
    int Speed,
    int MaxShields,
    string SkillName,
    IReadOnlySet<string> Weaknesses);

public sealed record SkillDefinition(
    string Name,
    int Sp,
    string Description,
    string Type,
    string Target,
    double Modifier,
    string Boost,
    int Hits);

public sealed record BeastSkillDefinition(
    string Name,
    double Modifier,
    string Description,
    string Target,
    int Hits);

public sealed record PassiveSkillDefinition(
    string Name,
    string Description,
    string Target);

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

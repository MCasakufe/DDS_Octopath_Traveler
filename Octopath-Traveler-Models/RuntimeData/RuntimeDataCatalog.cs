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
    public bool TryGetActiveSkill(string skillName, out SkillDefinition? skillDefinition)
        => ActiveSkillsByName.TryGetValue(skillName, out skillDefinition);

    public bool TryGetBeastSkill(string skillName, out BeastSkillDefinition? skillDefinition)
        => BeastSkillsByName.TryGetValue(skillName, out skillDefinition);

    public bool TryGetPassiveSkill(string skillName, out PassiveSkillDefinition? skillDefinition)
        => PassiveSkillsByName.TryGetValue(skillName, out skillDefinition);
}

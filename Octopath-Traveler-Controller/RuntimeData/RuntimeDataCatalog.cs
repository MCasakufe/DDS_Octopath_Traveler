namespace Octopath_Traveler.RuntimeData;

public sealed record TravelerDefinition(
    string Name,
    int MaxHp,
    int MaxSp,
    int PhysAtk,
    int PhysDef,
    int Speed,
    IReadOnlyList<string> Weapons);

public sealed record BeastDefinition(
    string Name,
    int MaxHp,
    int PhysAtk,
    int PhysDef,
    int Speed,
    int MaxShields,
    string SkillName);

public sealed record RuntimeDataCatalog(
    IReadOnlyDictionary<string, TravelerDefinition> TravelersByName,
    IReadOnlyDictionary<string, BeastDefinition> BeastsByName,
    IReadOnlySet<string> ActiveSkillNames,
    IReadOnlySet<string> PassiveSkillNames,
    IReadOnlySet<string> BeastSkillNames);
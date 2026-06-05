namespace Octopath_Traveler_Models.RuntimeData;

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

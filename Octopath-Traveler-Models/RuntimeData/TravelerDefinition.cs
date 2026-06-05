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

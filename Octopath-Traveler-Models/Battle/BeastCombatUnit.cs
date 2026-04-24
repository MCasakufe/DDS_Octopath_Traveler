using Octopath_Traveler_Models.RuntimeData;

namespace Octopath_Traveler_Models.Battle;

public sealed class BeastCombatUnit
    : Unit
{
    public BeastCombatUnit(BeastDefinition beastDefinition, int boardSlotIndex)
        : base(
            beastDefinition.Name,
            beastDefinition.MaxHp,
            beastDefinition.PhysAtk,
            beastDefinition.PhysDef,
            beastDefinition.Speed,
            boardSlotIndex)
    {
        MaxShields = beastDefinition.MaxShields;
        CurrentShields = beastDefinition.MaxShields;
        SkillName = beastDefinition.SkillName;
    }

    public int CurrentShields { get; set; }

    public int MaxShields { get; }

    public string SkillName { get; }
}

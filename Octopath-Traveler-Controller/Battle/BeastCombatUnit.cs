using Octopath_Traveler.RuntimeData;

namespace Octopath_Traveler.Battle;

public sealed class BeastCombatUnit
{
    public BeastCombatUnit(BeastDefinition beastDefinition, int boardSlotIndex)
    {
        Name = beastDefinition.Name;
        MaxHp = beastDefinition.MaxHp;
        CurrentHp = beastDefinition.MaxHp;
        PhysAtk = beastDefinition.PhysAtk;
        PhysDef = beastDefinition.PhysDef;
        Speed = beastDefinition.Speed;
        MaxShields = beastDefinition.MaxShields;
        CurrentShields = beastDefinition.MaxShields;
        SkillName = beastDefinition.SkillName;
        BoardSlotIndex = boardSlotIndex;
    }

    public string Name { get; }

    public int MaxHp { get; }

    public int CurrentHp { get; set; }

    public int PhysAtk { get; }

    public int PhysDef { get; }

    public int Speed { get; }

    public int CurrentShields { get; set; }

    public int MaxShields { get; }

    public string SkillName { get; }

    public int BoardSlotIndex { get; }

    public bool IsAlive => CurrentHp > 0;
}
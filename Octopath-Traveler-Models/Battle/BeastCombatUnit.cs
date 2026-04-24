using Octopath_Traveler_Models.RuntimeData;

namespace Octopath_Traveler_Models.Battle;

public sealed class BeastCombatUnit
    : Unit
{
    public BeastCombatUnit(
        BeastDefinition beastDefinition,
        BeastSkillDefinition assignedSkill,
        int boardSlotIndex)
        : base(
            beastDefinition.Name,
            beastDefinition.MaxHp,
            beastDefinition.PhysAtk,
            beastDefinition.PhysDef,
            beastDefinition.ElemAtk,
            beastDefinition.ElemDef,
            beastDefinition.Speed,
            boardSlotIndex)
    {
        MaxShields = beastDefinition.MaxShields;
        CurrentShields = beastDefinition.MaxShields;
        SkillName = beastDefinition.SkillName;
        AssignedSkill = assignedSkill;
        Weaknesses = beastDefinition.Weaknesses;
    }

    public int CurrentShields { get; set; }

    public int MaxShields { get; }

    public string SkillName { get; }

    public BeastSkillDefinition AssignedSkill { get; }

    public IReadOnlySet<string> Weaknesses { get; }

    public int RemainingBreakingRounds { get; set; }

    public bool HasRecoveryPriorityCurrentRound { get; set; }

    public int RemainingDecreasedPriorityRounds { get; set; }
}

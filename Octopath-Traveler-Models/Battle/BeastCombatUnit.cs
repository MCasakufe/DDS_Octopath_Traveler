using Octopath_Traveler_Models.RuntimeData;

namespace Octopath_Traveler_Models.Battle;

public sealed class BeastCombatUnit
    : Unit
{
    private const int NoShields = 0;

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

    public string GetAssignedSkillName()
        => AssignedSkill.Name;

    public string GetAssignedSkillTargetType()
        => AssignedSkill.Target;

    public int GetAssignedSkillHits()
        => AssignedSkill.Hits;

    public double GetAssignedSkillModifier()
        => AssignedSkill.Modifier;

    public void ApplyDecreasedPriorityRounds(int rounds)
    {
        if (rounds <= 0)
            return;

        RemainingDecreasedPriorityRounds += rounds;
    }

    public void ReceiveDamage(int damage)
    {
        int normalizedDamage = Math.Max(0, damage);
        CurrentHp = Math.Max(0, CurrentHp - normalizedDamage);
    }

    public void ConsumeShield()
    {
        if (CurrentShields <= NoShields)
            return;

        CurrentShields -= 1;
    }

    public bool HasNoShieldsRemaining()
        => CurrentShields <= NoShields;

    public void EnterBreakingPoint(int breakingRoundsDuration)
    {
        CurrentShields = NoShields;
        RemainingBreakingRounds = breakingRoundsDuration;
        HasRecoveryPriorityCurrentRound = false;
    }

    public void PrepareRoundStateForNextRound()
    {
        HasRecoveryPriorityCurrentRound = false;
        DecreaseBreakingRoundsAndRecoverShieldsIfNeeded();
        DecreasePriorityPenalty();
    }

    private void DecreaseBreakingRoundsAndRecoverShieldsIfNeeded()
    {
        if (RemainingBreakingRounds <= 0)
            return;

        RemainingBreakingRounds -= 1;
        if (RemainingBreakingRounds == 0 && IsAlive)
        {
            CurrentShields = MaxShields;
            HasRecoveryPriorityCurrentRound = true;
        }
    }

    private void DecreasePriorityPenalty()
    {
        if (RemainingDecreasedPriorityRounds > 0)
            RemainingDecreasedPriorityRounds -= 1;
    }
}

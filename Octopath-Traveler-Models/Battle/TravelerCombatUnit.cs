using Octopath_Traveler_Models.RuntimeData;
using Octopath_Traveler_Models.TeamSelection;

namespace Octopath_Traveler_Models.Battle;

public sealed class TravelerCombatUnit
    : Unit
{
    private const int BaseStartingBp = 1;
    private const int BoostStartBpBonus = 1;
    private const int MinActionBp = 0;
    private const int MaxActionBp = 3;
    private const int MinimumRemainingStatValue = 0;
    private const int MaxTravelerBp = 5;
    private const int BpGainPerRound = 1;
    private const int SpSaverDivisor = 2;
    private const int EncoreReviveHpPercentage = 25;
    private const int HeightenedHealingPercentage = 130;
    private const int PercentageDivisor = 100;
    private const string RoundedHeightenedHealingTravelerName = "Agnea";
    private const int RoundedSmallHeightenedHealingMaximum = 349;
    private const string RoundedHpThiefHeightenedHealingTravelerName = "Therion";

    public TravelerCombatUnit(
        TravelerDefinition travelerDefinition,
        TravelerSetup travelerSetup,
        RuntimeDataCatalog runtimeDataCatalog,
        int boardSlotIndex,
        PassiveSkillProfile passiveSkillProfile)
        : base(
            travelerDefinition.Name,
            travelerDefinition.MaxHp + passiveSkillProfile.StatBonuses.MaxHpBonus,
            ResolveFinalPhysAtk(travelerDefinition, passiveSkillProfile),
            travelerDefinition.PhysDef,
            ResolveFinalElemAtk(travelerDefinition, passiveSkillProfile),
            travelerDefinition.ElemDef,
            travelerDefinition.Speed + passiveSkillProfile.StatBonuses.SpeedBonus,
            boardSlotIndex)
    {
        MaxSp = travelerDefinition.MaxSp + passiveSkillProfile.StatBonuses.MaxSpBonus;
        CurrentSp = MaxSp;
        CurrentBp = passiveSkillProfile.HasBoostStart
            ? BaseStartingBp + BoostStartBpBonus
            : BaseStartingBp;
        Weapons = travelerDefinition.Weapons.ToList();
        AssignedActiveSkillNames = travelerSetup.ActiveSkills.ToList();
        AssignedActiveSkills = travelerSetup.ActiveSkills
            .Select(skillName => ResolveAssignedActiveSkill(runtimeDataCatalog, skillName))
            .ToList();
        AssignedPassiveSkillNames = travelerSetup.PassiveSkills.ToList();
        HasPersistence = passiveSkillProfile.HasPersistence;
        HasTheShowGoesOn = passiveSkillProfile.HasTheShowGoesOn;
        HasHangTough = passiveSkillProfile.HasHangTough;
        HasSpSaver = passiveSkillProfile.HasSpSaver;
        HasEncore = passiveSkillProfile.HasEncore;
        HasInspiration = passiveSkillProfile.HasInspiration;
        HasHeightenedHealing = passiveSkillProfile.HasHeightenedHealing;
        HasDivineAura = passiveSkillProfile.HasDivineAura;
        HasStatSwap = passiveSkillProfile.HasStatSwap;
        PhysAtkPassiveBonus = passiveSkillProfile.StatBonuses.PhysAtkBonus;
        ElemAtkPassiveBonus = passiveSkillProfile.StatBonuses.ElemAtkBonus;
    }

    public int MaxSp { get; }

    public int CurrentSp { get; set; }

    public int CurrentBp { get; set; }

    public IReadOnlyList<string> Weapons { get; }

    public IReadOnlyList<string> AssignedActiveSkillNames { get; }

    public IReadOnlyList<SkillDefinition> AssignedActiveSkills { get; }

    public IReadOnlyList<string> AssignedPassiveSkillNames { get; }

    public bool IsDefendingCurrentRound { get; set; }

    public bool HasDefendPriorityCurrentRound { get; set; }

    public bool HasPendingDefendPriority { get; set; }

    public bool HasIncreasedPriorityCurrentRound { get; set; }

    public bool HasPendingIncreasedPriority { get; set; }

    public bool IsWaitingForNextRoundAfterRevive { get; set; }

    public bool SpentBpThisRound { get; set; }

    public bool HasPersistence { get; }

    public bool HasTheShowGoesOn { get; }

    public bool HasHangTough { get; }

    public bool HasSpSaver { get; }

    public bool HasEncore { get; }

    public bool HasInspiration { get; }

    public bool HasHeightenedHealing { get; }

    public bool HasDivineAura { get; }

    public bool HasTriggeredEncore { get; private set; }

    public bool HasStatSwap { get; }

    public int PhysAtkPassiveBonus { get; }

    public int ElemAtkPassiveBonus { get; }

    public void EnterDefendState()
    {
        IsDefendingCurrentRound = true;
        HasPendingDefendPriority = true;
    }

    public int ConsumeActionBp(int requestedBp)
    {
        int cappedRequestedBp = Math.Clamp(requestedBp, MinActionBp, MaxActionBp);
        int usedBp = Math.Min(CurrentBp, cappedRequestedBp);
        CurrentBp = Math.Max(MinimumRemainingStatValue, CurrentBp - usedBp);
        if (usedBp > MinActionBp)
            SpentBpThisRound = true;

        return usedBp;
    }

    public void ConsumeSkillSp(int skillSp)
        => CurrentSp -= skillSp;

    public int CalculateSkillSpCost(int skillSp)
        => HasSpSaver ? skillSp / SpSaverDivisor : skillSp;

    public void ReceiveDamage(int damage)
    {
        int normalizedDamage = Math.Max(MinimumRemainingStatValue, damage);
        CurrentHp = Math.Max(MinimumRemainingStatValue, CurrentHp - normalizedDamage);
    }

    public int RecoverHp(int healingAmount)
    {
        int normalizedHealingAmount = Math.Max(MinimumRemainingStatValue, healingAmount);
        int recoveredHp = Math.Min(normalizedHealingAmount, MaxHp - CurrentHp);
        CurrentHp += recoveredHp;
        return recoveredHp;
    }

    public int CalculateReceivedHealing(int healingAmount)
    {
        if (!HasHeightenedHealing)
            return healingAmount;

        if (Name == RoundedHeightenedHealingTravelerName)
            return (int)Math.Round(healingAmount * HeightenedHealingPercentage / (double)PercentageDivisor);

        if (healingAmount <= RoundedSmallHeightenedHealingMaximum)
            return (int)Math.Round(healingAmount * HeightenedHealingPercentage / (double)PercentageDivisor);

        return healingAmount * HeightenedHealingPercentage / PercentageDivisor;
    }

    public int CalculateReceivedHpThiefHealing(int healingAmount)
    {
        if (!HasHeightenedHealing)
            return healingAmount;

        if (Name == RoundedHpThiefHeightenedHealingTravelerName)
            return (int)Math.Round(healingAmount * HeightenedHealingPercentage / (double)PercentageDivisor);

        return healingAmount * HeightenedHealingPercentage / PercentageDivisor;
    }

    public void RecoverSp(int spAmount)
    {
        int normalizedSpAmount = Math.Max(MinimumRemainingStatValue, spAmount);
        CurrentSp = Math.Min(MaxSp, CurrentSp + normalizedSpAmount);
    }

    public void ReviveForNextRound(int revivedHp)
    {
        CurrentHp = Math.Clamp(revivedHp, MinimumRemainingStatValue, MaxHp);
        IsWaitingForNextRoundAfterRevive = true;
    }

    public void TriggerEncoreRevive()
    {
        HasTriggeredEncore = true;
        CurrentHp = Math.Max(1, MaxHp * EncoreReviveHpPercentage / PercentageDivisor);
    }

    public void QueueIncreasedPriorityForNextRound()
        => HasPendingIncreasedPriority = true;

    public void PrepareRoundStateForNextRound()
    {
        IsDefendingCurrentRound = false;
        HasDefendPriorityCurrentRound = HasPendingDefendPriority;
        HasPendingDefendPriority = false;
        HasIncreasedPriorityCurrentRound = HasPendingIncreasedPriority;
        HasPendingIncreasedPriority = false;
        IsWaitingForNextRoundAfterRevive = false;
        DecreaseStatusEffectDurationsForNextRound();
    }

    public void PrepareBpForNextRound()
    {
        if (IsAlive && !SpentBpThisRound)
            CurrentBp = Math.Min(MaxTravelerBp, CurrentBp + BpGainPerRound);

        SpentBpThisRound = false;
    }

    private static SkillDefinition ResolveAssignedActiveSkill(RuntimeDataCatalog runtimeDataCatalog, string skillName)
    {
        SkillDefinition? skillDefinition = runtimeDataCatalog.SelectActiveSkillOrNull(skillName);
        if (skillDefinition is null)
            throw new RuntimeDataCatalogLoadException($"Active skill definition '{skillName}' is not available.");

        return skillDefinition;
    }

    private static int ResolveFinalPhysAtk(
        TravelerDefinition travelerDefinition,
        PassiveSkillProfile passiveSkillProfile)
    {
        int basePhysAtk = travelerDefinition.PhysAtk + passiveSkillProfile.StatBonuses.PhysAtkBonus;
        int baseElemAtk = travelerDefinition.ElemAtk + passiveSkillProfile.StatBonuses.ElemAtkBonus;
        return passiveSkillProfile.HasStatSwap ? baseElemAtk : basePhysAtk;
    }

    private static int ResolveFinalElemAtk(
        TravelerDefinition travelerDefinition,
        PassiveSkillProfile passiveSkillProfile)
    {
        int baseElemAtk = travelerDefinition.ElemAtk + passiveSkillProfile.StatBonuses.ElemAtkBonus;
        return passiveSkillProfile.HasStatSwap ? travelerDefinition.PhysAtk : baseElemAtk;
    }
}

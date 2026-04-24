using Octopath_Traveler_Models.RuntimeData;
using Octopath_Traveler_Models.TeamSelection;

namespace Octopath_Traveler_Models.Battle;

public sealed class TravelerCombatUnit
    : Unit
{
    private const int BaseStartingBp = 1;
    private const int BoostStartBpBonus = 1;

    private static readonly IReadOnlyDictionary<string, PassiveStatBonus> PassiveBonusesByName
        = new Dictionary<string, PassiveStatBonus>(StringComparer.Ordinal)
        {
            ["Elemental Augmentation"] = new(0, 0, 0, 50, 0),
            ["Summon Strength"] = new(0, 0, 50, 0, 0),
            ["Hale and Hearty"] = new(500, 0, 0, 0, 0),
            ["Fleefoot"] = new(0, 0, 0, 0, 50),
            ["Inner Strength"] = new(0, 50, 0, 0, 0)
        };

    public TravelerCombatUnit(
        TravelerDefinition travelerDefinition,
        TravelerSetup travelerSetup,
        RuntimeDataCatalog runtimeDataCatalog,
        int boardSlotIndex)
        : this(
            travelerDefinition,
            travelerSetup,
            runtimeDataCatalog,
            boardSlotIndex,
            AnalyzePassiveEffects(travelerSetup.PassiveSkills))
    {
    }

    private TravelerCombatUnit(
        TravelerDefinition travelerDefinition,
        TravelerSetup travelerSetup,
        RuntimeDataCatalog runtimeDataCatalog,
        int boardSlotIndex,
        PassiveEffects passiveEffects)
        : base(
            travelerDefinition.Name,
            travelerDefinition.MaxHp + passiveEffects.StatBonuses.MaxHpBonus,
            ResolveFinalPhysAtk(travelerDefinition, passiveEffects),
            travelerDefinition.PhysDef,
            ResolveFinalElemAtk(travelerDefinition, passiveEffects),
            travelerDefinition.ElemDef,
            travelerDefinition.Speed + passiveEffects.StatBonuses.SpeedBonus,
            boardSlotIndex)
    {
        MaxSp = travelerDefinition.MaxSp + passiveEffects.StatBonuses.MaxSpBonus;
        CurrentSp = MaxSp;
        CurrentBp = passiveEffects.HasBoostStart
            ? BaseStartingBp + BoostStartBpBonus
            : BaseStartingBp;
        HasVimAndVigor = passiveEffects.HasVimAndVigor;
        HasSecondWind = passiveEffects.HasSecondWind;
        Weapons = travelerDefinition.Weapons.ToList();
        AssignedActiveSkillNames = travelerSetup.ActiveSkills.ToList();
        AssignedActiveSkills = travelerSetup.ActiveSkills
            .Select(skillName => ResolveAssignedActiveSkill(runtimeDataCatalog, skillName))
            .ToList();
        AssignedPassiveSkillNames = travelerSetup.PassiveSkills.ToList();
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

    public bool HasVimAndVigor { get; }

    public bool HasSecondWind { get; }

    private static SkillDefinition ResolveAssignedActiveSkill(RuntimeDataCatalog runtimeDataCatalog, string skillName)
    {
        if (!runtimeDataCatalog.TryGetActiveSkill(skillName, out SkillDefinition? skillDefinition) || skillDefinition is null)
            throw new RuntimeDataCatalogLoadException($"Active skill definition '{skillName}' is not available.");

        return skillDefinition;
    }

    private static PassiveEffects AnalyzePassiveEffects(IEnumerable<string> passiveSkillNames)
    {
        bool hasBoostStart = false;
        bool hasStatSwap = false;
        bool hasVimAndVigor = false;
        bool hasSecondWind = false;
        PassiveStatBonus bonuses = PassiveStatBonus.None;

        foreach (string passiveSkillName in passiveSkillNames)
        {
            if (PassiveBonusesByName.TryGetValue(passiveSkillName, out PassiveStatBonus passiveBonus))
                bonuses = bonuses.Add(passiveBonus);

            hasBoostStart = hasBoostStart || passiveSkillName == "Boost Start";
            hasStatSwap = hasStatSwap || passiveSkillName == "Stat Swap";
            hasVimAndVigor = hasVimAndVigor || passiveSkillName == "Vim and Vigor";
            hasSecondWind = hasSecondWind || passiveSkillName == "Second Wind";
        }

        return new PassiveEffects(
            bonuses,
            hasBoostStart,
            hasStatSwap,
            hasVimAndVigor,
            hasSecondWind);
    }

    private static int ResolveFinalPhysAtk(TravelerDefinition travelerDefinition, PassiveEffects passiveEffects)
    {
        int basePhysAtk = travelerDefinition.PhysAtk + passiveEffects.StatBonuses.PhysAtkBonus;
        int baseElemAtk = travelerDefinition.ElemAtk + passiveEffects.StatBonuses.ElemAtkBonus;
        return passiveEffects.HasStatSwap ? baseElemAtk : basePhysAtk;
    }

    private static int ResolveFinalElemAtk(TravelerDefinition travelerDefinition, PassiveEffects passiveEffects)
    {
        int basePhysAtk = travelerDefinition.PhysAtk + passiveEffects.StatBonuses.PhysAtkBonus;
        int baseElemAtk = travelerDefinition.ElemAtk + passiveEffects.StatBonuses.ElemAtkBonus;
        return passiveEffects.HasStatSwap ? basePhysAtk : baseElemAtk;
    }

    private readonly record struct PassiveEffects(
        PassiveStatBonus StatBonuses,
        bool HasBoostStart,
        bool HasStatSwap,
        bool HasVimAndVigor,
        bool HasSecondWind);

    private readonly record struct PassiveStatBonus(
        int MaxHpBonus,
        int MaxSpBonus,
        int PhysAtkBonus,
        int ElemAtkBonus,
        int SpeedBonus)
    {
        public static PassiveStatBonus None => new(0, 0, 0, 0, 0);

        public PassiveStatBonus Add(PassiveStatBonus other)
        {
            return new PassiveStatBonus(
                MaxHpBonus + other.MaxHpBonus,
                MaxSpBonus + other.MaxSpBonus,
                PhysAtkBonus + other.PhysAtkBonus,
                ElemAtkBonus + other.ElemAtkBonus,
                SpeedBonus + other.SpeedBonus);
        }
    }
}

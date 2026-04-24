using Octopath_Traveler_Models.RuntimeData;
using Octopath_Traveler_Models.TeamSelection;

namespace Octopath_Traveler_Models.Battle;

public sealed class TravelerCombatUnit
    : Unit
{
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
            CalculatePassiveStatBonuses(travelerSetup.PassiveSkills))
    {
    }

    private TravelerCombatUnit(
        TravelerDefinition travelerDefinition,
        TravelerSetup travelerSetup,
        RuntimeDataCatalog runtimeDataCatalog,
        int boardSlotIndex,
        PassiveStatBonus passiveBonuses)
        : base(
            travelerDefinition.Name,
            travelerDefinition.MaxHp + passiveBonuses.MaxHpBonus,
            travelerDefinition.PhysAtk + passiveBonuses.PhysAtkBonus,
            travelerDefinition.PhysDef,
            travelerDefinition.ElemAtk + passiveBonuses.ElemAtkBonus,
            travelerDefinition.ElemDef,
            travelerDefinition.Speed + passiveBonuses.SpeedBonus,
            boardSlotIndex)
    {
        MaxSp = travelerDefinition.MaxSp + passiveBonuses.MaxSpBonus;
        CurrentSp = MaxSp;
        CurrentBp = 1;
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

    private static SkillDefinition ResolveAssignedActiveSkill(RuntimeDataCatalog runtimeDataCatalog, string skillName)
    {
        if (!runtimeDataCatalog.TryGetActiveSkill(skillName, out SkillDefinition? skillDefinition) || skillDefinition is null)
            throw new RuntimeDataCatalogLoadException($"Active skill definition '{skillName}' is not available.");

        return skillDefinition;
    }

    private static PassiveStatBonus CalculatePassiveStatBonuses(IEnumerable<string> passiveSkillNames)
    {
        PassiveStatBonus bonuses = PassiveStatBonus.None;
        foreach (string passiveSkillName in passiveSkillNames)
        {
            if (PassiveBonusesByName.TryGetValue(passiveSkillName, out PassiveStatBonus passiveBonus))
                bonuses = bonuses.Add(passiveBonus);
        }

        return bonuses;
    }

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

using Octopath_Traveler_Models.RuntimeData;
using Octopath_Traveler_Models.TeamSelection;

namespace Octopath_Traveler_Models.Battle;

public sealed class TravelerCombatUnit
    : Unit
{
    public TravelerCombatUnit(TravelerDefinition travelerDefinition, TravelerSetup travelerSetup, int boardSlotIndex)
        : base(
            travelerDefinition.Name,
            travelerDefinition.MaxHp,
            travelerDefinition.PhysAtk,
            travelerDefinition.PhysDef,
            travelerDefinition.Speed,
            boardSlotIndex)
    {
        MaxSp = travelerDefinition.MaxSp;
        CurrentSp = travelerDefinition.MaxSp;
        CurrentBp = 1;
        Weapons = travelerDefinition.Weapons.ToList();
        AssignedActiveSkillNames = travelerSetup.ActiveSkills.ToList();
        AssignedPassiveSkillNames = travelerSetup.PassiveSkills.ToList();
    }

    public int MaxSp { get; }

    public int CurrentSp { get; set; }

    public int CurrentBp { get; set; }

    public IReadOnlyList<string> Weapons { get; }

    public IReadOnlyList<string> AssignedActiveSkillNames { get; }

    public IReadOnlyList<string> AssignedPassiveSkillNames { get; }
}

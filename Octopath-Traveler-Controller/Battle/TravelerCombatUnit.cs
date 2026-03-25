using Octopath_Traveler.RuntimeData;
using Octopath_Traveler.TeamSelection;

namespace Octopath_Traveler.Battle;

public sealed class TravelerCombatUnit
{
    public TravelerCombatUnit(TravelerDefinition travelerDefinition, TravelerSetup travelerSetup, int boardSlotIndex)
    {
        Name = travelerDefinition.Name;
        MaxHp = travelerDefinition.MaxHp;
        CurrentHp = travelerDefinition.MaxHp;
        MaxSp = travelerDefinition.MaxSp;
        CurrentSp = travelerDefinition.MaxSp;
        CurrentBp = 1;
        PhysAtk = travelerDefinition.PhysAtk;
        PhysDef = travelerDefinition.PhysDef;
        Speed = travelerDefinition.Speed;
        Weapons = travelerDefinition.Weapons.ToList();
        AssignedActiveSkillNames = travelerSetup.ActiveSkills.ToList();
        AssignedPassiveSkillNames = travelerSetup.PassiveSkills.ToList();
        BoardSlotIndex = boardSlotIndex;
    }

    public string Name { get; }

    public int MaxHp { get; }

    public int CurrentHp { get; set; }

    public int MaxSp { get; }

    public int CurrentSp { get; set; }

    public int CurrentBp { get; set; }

    public int PhysAtk { get; }

    public int PhysDef { get; }

    public int Speed { get; }

    public IReadOnlyList<string> Weapons { get; }

    public IReadOnlyList<string> AssignedActiveSkillNames { get; }

    public IReadOnlyList<string> AssignedPassiveSkillNames { get; }

    public int BoardSlotIndex { get; }

    public bool IsAlive => CurrentHp > 0;
}
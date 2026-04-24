namespace Octopath_Traveler_Models.Battle;

public abstract class Unit
{
    protected Unit(
        string name,
        int maxHp,
        int physAtk,
        int physDef,
        int elemAtk,
        int elemDef,
        int speed,
        int boardSlotIndex)
    {
        Name = name;
        MaxHp = maxHp;
        CurrentHp = maxHp;
        PhysAtk = physAtk;
        PhysDef = physDef;
        ElemAtk = elemAtk;
        ElemDef = elemDef;
        Speed = speed;
        BoardSlotIndex = boardSlotIndex;
    }

    public string Name { get; }

    public int MaxHp { get; }

    public int CurrentHp { get; set; }

    public int PhysAtk { get; }

    public int PhysDef { get; }

    public int ElemAtk { get; }

    public int ElemDef { get; }

    public int Speed { get; }

    public int BoardSlotIndex { get; }

    public bool IsAlive => CurrentHp > 0;
}

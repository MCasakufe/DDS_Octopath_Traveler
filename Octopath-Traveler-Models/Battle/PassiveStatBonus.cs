namespace Octopath_Traveler_Models.Battle;

public readonly record struct PassiveStatBonus(
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

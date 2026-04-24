namespace Octopath_Traveler_Models.Battle;

public sealed record TravelerBasicAttack(
    string AttackerName,
    string TargetName,
    string WeaponType,
    int Damage,
    int TargetCurrentHp);

public sealed class TravelerBasicAttackExecutor
{
    private readonly PhysicalAttackDamageCalculator _damageCalculator;

    public TravelerBasicAttackExecutor(PhysicalAttackDamageCalculator damageCalculator)
    {
        _damageCalculator = damageCalculator;
    }

    public TravelerBasicAttack ExecuteAttack(TravelerCombatUnit traveler, BeastCombatUnit target, string weaponType)
    {
        var damage = _damageCalculator.CalculateDamage(traveler.PhysAtk, target.PhysDef);
        target.CurrentHp = Math.Max(0, target.CurrentHp - damage);

        return new TravelerBasicAttack(traveler.Name, target.Name, weaponType, damage, target.CurrentHp);
    }
}


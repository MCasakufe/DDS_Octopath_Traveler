namespace Octopath_Traveler.Battle;

public sealed record BeastAttack(
    string AttackerName,
    string TargetName,
    int Damage,
    int TargetCurrentHp);

public sealed class BeastAttackExecutor
{
    private readonly PhysicalAttackDamageCalculator _damageCalculator;

    public BeastAttackExecutor(PhysicalAttackDamageCalculator damageCalculator)
    {
        _damageCalculator = damageCalculator;
    }

    public BeastAttack? ExecuteAttack(BeastCombatUnit beast, BattleState battleState)
    {
        var target = SelectTargetTraveler(battleState);
        if (target is null)
            return null;

        var damage = _damageCalculator.CalculateDamage(beast.PhysAtk, target.PhysDef);
        target.CurrentHp = Math.Max(0, target.CurrentHp - damage);
        return new BeastAttack(beast.Name, target.Name, damage, target.CurrentHp);
    }

    private static TravelerCombatUnit? SelectTargetTraveler(BattleState battleState)
        => battleState.TravelerTeam
            .Where(traveler => traveler.IsAlive)
            .OrderByDescending(traveler => traveler.CurrentHp)
            .ThenBy(traveler => traveler.BoardSlotIndex)
            .FirstOrDefault();
}

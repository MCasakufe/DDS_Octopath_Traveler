namespace Octopath_Traveler_Models.Battle;

public sealed record PhysicalAttackOutcome(int Damage, int TargetCurrentHp);

public sealed class PhysicalAttackExecutionService
{
    private readonly PhysicalAttackDamageCalculator _damageCalculator;

    public PhysicalAttackExecutionService(PhysicalAttackDamageCalculator damageCalculator)
    {
        _damageCalculator = damageCalculator;
    }

    public PhysicalAttackOutcome Execute(int attackerPhysAtk, int targetPhysDef, int targetCurrentHp)
    {
        int damage = _damageCalculator.CalculateDamage(attackerPhysAtk, targetPhysDef);
        int updatedTargetHp = Math.Max(0, targetCurrentHp - damage);
        return new PhysicalAttackOutcome(damage, updatedTargetHp);
    }
}

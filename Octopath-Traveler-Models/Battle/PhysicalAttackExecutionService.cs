namespace Octopath_Traveler_Models.Battle;

public sealed class PhysicalAttackExecutionService
{
    private const int MinimumTargetHp = 0;

    private readonly PhysicalAttackDamageCalculator _damageCalculator;

    public PhysicalAttackExecutionService(PhysicalAttackDamageCalculator damageCalculator)
    {
        _damageCalculator = damageCalculator;
    }

    public PhysicalAttackOutcome Execute(int attackerPhysAtk, int targetPhysDef, int targetCurrentHp)
    {
        int damage = _damageCalculator.CalculateDamage(attackerPhysAtk, targetPhysDef);
        int updatedTargetHp = Math.Max(MinimumTargetHp, targetCurrentHp - damage);
        return new PhysicalAttackOutcome(damage, updatedTargetHp);
    }
}

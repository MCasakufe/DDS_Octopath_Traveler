namespace Octopath_Traveler_Models.Battle;

public sealed class PhysicalAttackDamageCalculator
{
    private const double AttackModifier = 1.3;

    public int CalculateDamage(int attackerPhysAtk, int targetPhysDef)
    {
        double rawDamage = Math.Floor(attackerPhysAtk * AttackModifier - targetPhysDef);
        return Math.Max(0, (int)rawDamage);
    }
}


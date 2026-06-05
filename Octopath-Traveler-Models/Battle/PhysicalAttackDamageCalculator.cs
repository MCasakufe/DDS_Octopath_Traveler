namespace Octopath_Traveler_Models.Battle;

public sealed class PhysicalAttackDamageCalculator
{
    private const double AttackModifier = 1.3;
    private const int MinimumDamage = 0;

    public int CalculateDamage(int attackerPhysAtk, int targetPhysDef)
    {
        double rawDamage = Math.Floor(attackerPhysAtk * AttackModifier - targetPhysDef);
        return Math.Max(MinimumDamage, (int)rawDamage);
    }
}


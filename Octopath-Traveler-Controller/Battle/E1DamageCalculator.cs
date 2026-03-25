namespace Octopath_Traveler.Battle;

public sealed class E1DamageCalculator
{
    private const double AttackModifier = 1.3;

    public int CalculateDamage(int attackerPhysAtk, int targetPhysDef)
    {
        var rawDamage = Math.Floor(attackerPhysAtk * AttackModifier - targetPhysDef);
        return Math.Max(0, (int)rawDamage);
    }
}
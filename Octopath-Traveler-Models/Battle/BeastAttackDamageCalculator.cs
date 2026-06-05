namespace Octopath_Traveler_Models.Battle;

internal sealed class BeastAttackDamageCalculator
{
    private const int MinimumDamage = 0;
    private const int HalfCurrentHpRoundingOffset = 1;
    private const int HalfCurrentHpDivisor = 2;
    private const int DefendDamageDivisor = 2;

    public int CalculateDamage(BeastAttackDamageRequest damageRequest)
    {
        if (damageRequest.DamageKind == BeastAttackDamageKind.HalfCurrentHp)
            return CalculateHalfCurrentHpDamage(damageRequest.Target);

        int attackerStat = SelectAttackerStat(damageRequest);
        int defenderStat = SelectDefenderStat(damageRequest);
        double attackerStatusMultiplier = SelectAttackerStatusMultiplier(damageRequest);
        double defenderStatusMultiplier = SelectDefenderStatusMultiplier(damageRequest);
        int baseDamage = CalculateBaseDamage(
            attackerStat,
            damageRequest.SkillModifier,
            defenderStat,
            attackerStatusMultiplier,
            defenderStatusMultiplier);
        return ApplyDefendReduction(damageRequest.Target, baseDamage);
    }

    private static int CalculateHalfCurrentHpDamage(TravelerCombatUnit target)
        => (target.CurrentHp + HalfCurrentHpRoundingOffset) / HalfCurrentHpDivisor;

    private static int SelectAttackerStat(BeastAttackDamageRequest damageRequest)
        => damageRequest.DamageKind == BeastAttackDamageKind.Elemental
            ? damageRequest.Attacker.ElemAtk
            : damageRequest.Attacker.PhysAtk;

    private static int SelectDefenderStat(BeastAttackDamageRequest damageRequest)
        => damageRequest.DamageKind == BeastAttackDamageKind.Elemental
            ? damageRequest.Target.ElemDef
            : damageRequest.Target.PhysDef;

    private static int CalculateBaseDamage(
        int attackerStat,
        double modifier,
        int defenderStat,
        double attackerStatusMultiplier,
        double defenderStatusMultiplier)
    {
        double rawDamage = (attackerStat * modifier - defenderStat)
                           * attackerStatusMultiplier
                           * defenderStatusMultiplier;
        return Math.Max(MinimumDamage, (int)Math.Floor(rawDamage));
    }

    private static double SelectAttackerStatusMultiplier(BeastAttackDamageRequest damageRequest)
        => damageRequest.DamageKind == BeastAttackDamageKind.Elemental
            ? damageRequest.Attacker.GetElementalAttackDamageMultiplier()
            : damageRequest.Attacker.GetPhysicalAttackDamageMultiplier();

    private static double SelectDefenderStatusMultiplier(BeastAttackDamageRequest damageRequest)
        => damageRequest.DamageKind == BeastAttackDamageKind.Elemental
            ? damageRequest.Target.GetElementalDefenseDamageMultiplier()
            : damageRequest.Target.GetPhysicalDefenseDamageMultiplier();

    private static int ApplyDefendReduction(TravelerCombatUnit target, int baseDamage)
        => target.IsDefendingCurrentRound ? baseDamage / DefendDamageDivisor : baseDamage;
}

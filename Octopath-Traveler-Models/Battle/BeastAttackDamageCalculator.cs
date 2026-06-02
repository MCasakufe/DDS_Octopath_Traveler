namespace Octopath_Traveler_Models.Battle;

internal sealed record BeastAttackDamageRequest(
    BeastCombatUnit Attacker,
    TravelerCombatUnit Target,
    double SkillModifier,
    BeastAttackDamageKind DamageKind);

internal sealed class BeastAttackDamageCalculator
{
    private const int HalfCurrentHpRoundingOffset = 1;
    private const int HalfCurrentHpDivisor = 2;

    public int CalculateDamage(BeastAttackDamageRequest damageRequest)
    {
        if (damageRequest.DamageKind == BeastAttackDamageKind.HalfCurrentHp)
            return CalculateHalfCurrentHpDamage(damageRequest.Target);

        int attackerStat = SelectAttackerStat(damageRequest);
        int defenderStat = SelectDefenderStat(damageRequest);
        int baseDamage = CalculateBaseDamage(attackerStat, damageRequest.SkillModifier, defenderStat);
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

    private static int CalculateBaseDamage(int attackerStat, double modifier, int defenderStat)
        => Math.Max(0, (int)Math.Floor(attackerStat * modifier - defenderStat));

    private static int ApplyDefendReduction(TravelerCombatUnit target, int baseDamage)
        => target.IsDefendingCurrentRound ? baseDamage / 2 : baseDamage;
}

namespace Octopath_Traveler_Models.Battle;

internal sealed record BeastAttackHitExecutionRequest(
    BeastCombatUnit Attacker,
    TravelerCombatUnit Target,
    BeastAttackDamageKind DamageKind);

internal sealed record BeastAttackHitResult(
    int Damage,
    bool WasDefended);

internal sealed class BeastAttackHitExecutor
{
    private readonly BeastAttackDamageCalculator _damageCalculator;
    private readonly TravelerDamageApplier _damageApplier;

    public BeastAttackHitExecutor(
        BeastAttackDamageCalculator damageCalculator,
        TravelerDamageApplier damageApplier)
    {
        _damageCalculator = damageCalculator;
        _damageApplier = damageApplier;
    }

    public BeastAttackHitResult ExecuteHit(BeastAttackHitExecutionRequest executionRequest)
    {
        int damage = CalculateDamage(executionRequest);
        _damageApplier.ApplyDamage(new TravelerDamageApplication(executionRequest.Target, damage));
        return new BeastAttackHitResult(damage, IsDefendedHit(executionRequest));
    }

    private int CalculateDamage(BeastAttackHitExecutionRequest executionRequest)
        => _damageCalculator.CalculateDamage(new BeastAttackDamageRequest(
            executionRequest.Attacker,
            executionRequest.Target,
            executionRequest.Attacker.GetAssignedSkillModifier(),
            executionRequest.DamageKind));

    private static bool IsDefendedHit(BeastAttackHitExecutionRequest executionRequest)
        => executionRequest.Target.IsDefendingCurrentRound
           && executionRequest.DamageKind != BeastAttackDamageKind.HalfCurrentHp;
}

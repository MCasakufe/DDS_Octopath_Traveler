namespace Octopath_Traveler_Models.Battle;

internal sealed record TravelerBasicAttackHitExecutionRequest(
    TravelerCombatUnit Traveler,
    BeastCombatUnit Target,
    string WeaponType);

internal sealed class TravelerBasicAttackHitExecutor
{
    private const double BasicAttackModifier = 1.3;

    private readonly BeastDamageResolver _beastDamageResolver;

    public TravelerBasicAttackHitExecutor(BeastDamageResolver beastDamageResolver)
    {
        _beastDamageResolver = beastDamageResolver;
    }

    public TravelerBasicAttackHit ExecuteHit(TravelerBasicAttackHitExecutionRequest executionRequest)
    {
        BeastDamageResolution damageResolution = _beastDamageResolver.ResolveHit(BuildHitRequest(executionRequest));
        return new TravelerBasicAttackHit(
            damageResolution.Damage,
            damageResolution.IsWeaknessHit,
            damageResolution.EnteredBreakingPoint);
    }

    private static BeastHitRequest BuildHitRequest(TravelerBasicAttackHitExecutionRequest executionRequest)
        => new(
            executionRequest.Traveler.PhysAtk,
            executionRequest.Traveler.ElemAtk,
            executionRequest.Target,
            executionRequest.WeaponType,
            BasicAttackModifier);
}

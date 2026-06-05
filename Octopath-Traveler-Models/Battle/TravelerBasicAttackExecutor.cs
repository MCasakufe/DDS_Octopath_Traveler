namespace Octopath_Traveler_Models.Battle;

public sealed class TravelerBasicAttackExecutor
{
    private const int BasicAttackBaseHitCount = 1;
    private const int MinimumUsedBp = 0;
    private const int InspirationRecoveryPercentage = 1;
    private const int PercentageDivisor = 100;

    private readonly TravelerBasicAttackHitExecutor _hitExecutor;

    public TravelerBasicAttackExecutor(PhysicalAttackExecutionService physicalAttackExecutionService)
    {
        _ = physicalAttackExecutionService;
        _hitExecutor = new TravelerBasicAttackHitExecutor(new BeastDamageResolver());
    }

    public TravelerBasicAttack ExecuteAttack(TravelerBasicAttackExecutionRequest executionRequest)
    {
        IReadOnlyList<TravelerBasicAttackHit> hits = ApplyHits(executionRequest);
        TravelerBasicAttackSpRecoveryResult? spRecoveryResult = ApplyInspirationRecovery(
            executionRequest.Traveler,
            hits);

        return new TravelerBasicAttack(
            executionRequest.Traveler.Name,
            executionRequest.Target.Name,
            executionRequest.WeaponType,
            hits,
            spRecoveryResult,
            executionRequest.Target.CurrentHp);
    }

    private static TravelerBasicAttackSpRecoveryResult? ApplyInspirationRecovery(
        TravelerCombatUnit traveler,
        IReadOnlyList<TravelerBasicAttackHit> hits)
    {
        if (!traveler.HasInspiration)
            return null;

        int recoveredSp = CalculateInspirationRecovery(hits);
        if (recoveredSp <= 0)
            return null;

        traveler.RecoverSp(recoveredSp);
        return new TravelerBasicAttackSpRecoveryResult(traveler.Name, recoveredSp);
    }

    private static int CalculateInspirationRecovery(IEnumerable<TravelerBasicAttackHit> hits)
        => hits.Sum(hit => hit.Damage) * InspirationRecoveryPercentage / PercentageDivisor;

    private IReadOnlyList<TravelerBasicAttackHit> ApplyHits(TravelerBasicAttackExecutionRequest executionRequest)
    {
        List<TravelerBasicAttackHit> hits = [];
        for (int hitIndex = 0; hitIndex < CalculateHitCount(executionRequest.UsedBp); hitIndex++)
            hits.Add(ApplyHit(executionRequest));

        return hits;
    }

    private TravelerBasicAttackHit ApplyHit(TravelerBasicAttackExecutionRequest executionRequest)
        => _hitExecutor.ExecuteHit(new TravelerBasicAttackHitExecutionRequest(
            executionRequest.Traveler,
            executionRequest.Target,
            executionRequest.WeaponType));

    private static int CalculateHitCount(int usedBp)
        => BasicAttackBaseHitCount + Math.Max(MinimumUsedBp, usedBp);
}


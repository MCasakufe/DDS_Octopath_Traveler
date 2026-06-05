namespace Octopath_Traveler_Models.Battle;

public sealed class TravelerBasicAttackExecutor
{
    private const int BasicAttackBaseHitCount = 1;
    private const int MinimumUsedBp = 0;

    private readonly TravelerBasicAttackHitExecutor _hitExecutor;

    public TravelerBasicAttackExecutor(PhysicalAttackExecutionService physicalAttackExecutionService)
    {
        _ = physicalAttackExecutionService;
        _hitExecutor = new TravelerBasicAttackHitExecutor(new BeastDamageResolver());
    }

    public TravelerBasicAttack ExecuteAttack(TravelerBasicAttackExecutionRequest executionRequest)
    {
        IReadOnlyList<TravelerBasicAttackHit> hits = ApplyHits(executionRequest);

        return new TravelerBasicAttack(
            executionRequest.Traveler.Name,
            executionRequest.Target.Name,
            executionRequest.WeaponType,
            hits,
            executionRequest.Target.CurrentHp);
    }

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


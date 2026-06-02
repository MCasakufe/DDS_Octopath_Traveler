namespace Octopath_Traveler_Models.Battle;

public sealed record TravelerBasicAttack(
    string AttackerName,
    string TargetName,
    string WeaponType,
    IReadOnlyList<TravelerBasicAttackHit> Hits,
    int TargetCurrentHp);

public sealed record TravelerBasicAttackHit(
    int Damage,
    bool IsWeaknessHit,
    bool EnteredBreakingPoint);

public sealed record TravelerBasicAttackExecutionRequest(
    TravelerCombatUnit Traveler,
    BeastCombatUnit Target,
    string WeaponType,
    int UsedBp);

public sealed class TravelerBasicAttackExecutor
{
    private readonly TravelerBasicAttackHitExecutor _hitExecutor;

    public TravelerBasicAttackExecutor(PhysicalAttackExecutionService physicalAttackExecutionService)
    {
        _ = physicalAttackExecutionService;
        _hitExecutor = new TravelerBasicAttackHitExecutor(new BeastDamageResolver());
    }

    public TravelerBasicAttack ExecuteAttack(TravelerBasicAttackExecutionRequest executionRequest)
    {
        IReadOnlyList<TravelerBasicAttackHit> hits = ExecuteHits(executionRequest);

        return new TravelerBasicAttack(
            executionRequest.Traveler.Name,
            executionRequest.Target.Name,
            executionRequest.WeaponType,
            hits,
            executionRequest.Target.CurrentHp);
    }

    private IReadOnlyList<TravelerBasicAttackHit> ExecuteHits(TravelerBasicAttackExecutionRequest executionRequest)
    {
        List<TravelerBasicAttackHit> hits = [];
        for (int hitIndex = 0; hitIndex < CalculateHitCount(executionRequest.UsedBp); hitIndex++)
            hits.Add(ExecuteHit(executionRequest));

        return hits;
    }

    private TravelerBasicAttackHit ExecuteHit(TravelerBasicAttackExecutionRequest executionRequest)
        => _hitExecutor.ExecuteHit(new TravelerBasicAttackHitExecutionRequest(
            executionRequest.Traveler,
            executionRequest.Target,
            executionRequest.WeaponType));

    private static int CalculateHitCount(int usedBp)
        => 1 + Math.Max(0, usedBp);
}


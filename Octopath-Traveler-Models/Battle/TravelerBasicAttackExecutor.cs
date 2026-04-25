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
    private const double BasicAttackModifier = 1.3;

    private readonly BeastDamageResolver _beastDamageResolver;

    public TravelerBasicAttackExecutor(PhysicalAttackExecutionService physicalAttackExecutionService)
    {
        _ = physicalAttackExecutionService;
        _beastDamageResolver = new BeastDamageResolver();
    }

    public TravelerBasicAttack ExecuteAttack(TravelerBasicAttackExecutionRequest executionRequest)
    {
        TravelerCombatUnit traveler = executionRequest.Traveler;
        BeastCombatUnit target = executionRequest.Target;
        string weaponType = executionRequest.WeaponType;
        int usedBp = executionRequest.UsedBp;

        int boostHits = Math.Max(0, usedBp);
        int hitCount = 1 + boostHits;
        List<TravelerBasicAttackHit> hits = [];
        BeastHitRequest hitRequest = BuildBasicAttackHitRequest(traveler, target, weaponType);
        for (int hitIndex = 0; hitIndex < hitCount; hitIndex++)
        {
            BeastDamageResolution attackOutcome = _beastDamageResolver.ResolveHit(hitRequest);
            hits.Add(new TravelerBasicAttackHit(
                attackOutcome.Damage,
                attackOutcome.IsWeaknessHit,
                attackOutcome.EnteredBreakingPoint));
        }

        return new TravelerBasicAttack(
            traveler.Name,
            target.Name,
            weaponType,
            hits,
            target.CurrentHp);
    }

    private static BeastHitRequest BuildBasicAttackHitRequest(
        TravelerCombatUnit traveler,
        BeastCombatUnit target,
        string weaponType)
        => new(
            traveler.PhysAtk,
            traveler.ElemAtk,
            target,
            weaponType,
            BasicAttackModifier);
}


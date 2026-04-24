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

public sealed class TravelerBasicAttackExecutor
{
    private const double BasicAttackModifier = 1.3;

    private readonly BeastDamageResolver _beastDamageResolver;

    public TravelerBasicAttackExecutor(PhysicalAttackExecutionService physicalAttackExecutionService)
    {
        _ = physicalAttackExecutionService;
        _beastDamageResolver = new BeastDamageResolver();
    }

    public TravelerBasicAttack ExecuteAttack(TravelerCombatUnit traveler, BeastCombatUnit target, string weaponType, int usedBp)
    {
        int boostHits = Math.Max(0, usedBp);
        int hitCount = 1 + boostHits;
        List<TravelerBasicAttackHit> hits = [];
        for (int hitIndex = 0; hitIndex < hitCount; hitIndex++)
        {
            BeastDamageResolution attackOutcome = _beastDamageResolver.ResolveHit(
                traveler.PhysAtk,
                traveler.ElemAtk,
                target,
                weaponType,
                BasicAttackModifier);
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
}


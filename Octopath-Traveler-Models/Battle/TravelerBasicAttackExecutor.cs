namespace Octopath_Traveler_Models.Battle;

public sealed record TravelerBasicAttack(
    string AttackerName,
    string TargetName,
    string WeaponType,
    int Damage,
    int TargetCurrentHp,
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

    public TravelerBasicAttack ExecuteAttack(TravelerCombatUnit traveler, BeastCombatUnit target, string weaponType)
    {
        BeastDamageResolution attackOutcome = _beastDamageResolver.ResolveHit(
            traveler.PhysAtk,
            traveler.ElemAtk,
            target,
            weaponType,
            BasicAttackModifier);

        return new TravelerBasicAttack(
            traveler.Name,
            target.Name,
            weaponType,
            attackOutcome.Damage,
            attackOutcome.TargetCurrentHp,
            attackOutcome.IsWeaknessHit,
            attackOutcome.EnteredBreakingPoint);
    }
}


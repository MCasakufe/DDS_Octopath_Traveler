namespace Octopath_Traveler_Models.Battle;

public sealed record BeastAttack(
    string AttackerName,
    string TargetName,
    int Damage,
    int TargetCurrentHp);

public sealed class BeastAttackExecutor
{
    private readonly PhysicalAttackExecutionService _physicalAttackExecutionService;

    public BeastAttackExecutor(PhysicalAttackExecutionService physicalAttackExecutionService)
    {
        _physicalAttackExecutionService = physicalAttackExecutionService;
    }

    public BeastAttack? ExecuteAttack(BeastCombatUnit beast, BattleState battleState)
    {
        TravelerCombatUnit? target = SelectTargetTraveler(battleState);
        if (target is null)
            return null;

        PhysicalAttackOutcome attackOutcome = _physicalAttackExecutionService.Execute(
            beast.PhysAtk,
            target.PhysDef,
            target.CurrentHp);
        target.CurrentHp = attackOutcome.TargetCurrentHp;
        return new BeastAttack(beast.Name, target.Name, attackOutcome.Damage, attackOutcome.TargetCurrentHp);
    }

    private static TravelerCombatUnit? SelectTargetTraveler(BattleState battleState)
    {
        IEnumerable<TravelerCombatUnit> aliveTravelers = GetAliveTravelers(battleState);
        IOrderedEnumerable<TravelerCombatUnit> orderedTravelers = aliveTravelers
            .OrderByDescending(traveler => traveler.CurrentHp)
            .ThenBy(traveler => traveler.BoardSlotIndex);

        return orderedTravelers.FirstOrDefault();
    }

    private static IEnumerable<TravelerCombatUnit> GetAliveTravelers(BattleState battleState)
        => battleState.TravelerTeam.Where(traveler => traveler.IsAlive);
}


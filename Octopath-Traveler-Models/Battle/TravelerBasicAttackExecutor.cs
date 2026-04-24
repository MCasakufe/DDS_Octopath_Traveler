namespace Octopath_Traveler_Models.Battle;

public sealed record TravelerBasicAttack(
    string AttackerName,
    string TargetName,
    string WeaponType,
    int Damage,
    int TargetCurrentHp);

public sealed class TravelerBasicAttackExecutor
{
    private readonly PhysicalAttackExecutionService _physicalAttackExecutionService;

    public TravelerBasicAttackExecutor(PhysicalAttackExecutionService physicalAttackExecutionService)
    {
        _physicalAttackExecutionService = physicalAttackExecutionService;
    }

    public TravelerBasicAttack ExecuteAttack(TravelerCombatUnit traveler, BeastCombatUnit target, string weaponType)
    {
        PhysicalAttackOutcome attackOutcome = _physicalAttackExecutionService.Execute(
            traveler.PhysAtk,
            target.PhysDef,
            target.CurrentHp);
        target.CurrentHp = attackOutcome.TargetCurrentHp;

        return new TravelerBasicAttack(
            traveler.Name,
            target.Name,
            weaponType,
            attackOutcome.Damage,
            attackOutcome.TargetCurrentHp);
    }
}


using Octopath_Traveler_View;

namespace Octopath_Traveler.Battle;

public sealed class BeastAttackResolver
{
    private const string SeparatorLine = "----------------------------------------";

    private readonly View _view;
    private readonly E1DamageCalculator _damageCalculator;

    public BeastAttackResolver(View view, E1DamageCalculator damageCalculator)
    {
        _view = view;
        _damageCalculator = damageCalculator;
    }

    public void Resolve(BeastCombatUnit beast, BattleState battleState)
    {
        var target = SelectTargetTraveler(battleState);
        if (target is null)
            return;

        var damage = _damageCalculator.CalculateDamage(beast.PhysAtk, target.PhysDef);
        target.CurrentHp = Math.Max(0, target.CurrentHp - damage);

        _view.WriteLine(SeparatorLine);
        _view.WriteLine($"{beast.Name} usa Attack");
        _view.WriteLine($"{target.Name} recibe {damage} de daño físico");
        _view.WriteLine($"{target.Name} termina con HP:{target.CurrentHp}");
    }

    private static TravelerCombatUnit? SelectTargetTraveler(BattleState battleState)
        => battleState.TravelerTeam
            .Where(traveler => traveler.IsAlive)
            .OrderByDescending(traveler => traveler.CurrentHp)
            .ThenBy(traveler => traveler.BoardSlotIndex)
            .FirstOrDefault();
}
using Octopath_Traveler_View;

namespace Octopath_Traveler.Battle;

public sealed class TravelerBasicAttackResolver
{
    private const string SeparatorLine = "----------------------------------------";

    private readonly View _view;
    private readonly E1DamageCalculator _damageCalculator;

    public TravelerBasicAttackResolver(View view, E1DamageCalculator damageCalculator)
    {
        _view = view;
        _damageCalculator = damageCalculator;
    }

    public void Resolve(TravelerCombatUnit traveler, BeastCombatUnit target, string weaponType)
    {
        var damage = _damageCalculator.CalculateDamage(traveler.PhysAtk, target.PhysDef);
        target.CurrentHp = Math.Max(0, target.CurrentHp - damage);

        _view.WriteLine(SeparatorLine);
        _view.WriteLine($"{traveler.Name} ataca");
        _view.WriteLine($"{target.Name} recibe {damage} de daño de tipo {weaponType}");
        _view.WriteLine($"{target.Name} termina con HP:{target.CurrentHp}");
    }
}
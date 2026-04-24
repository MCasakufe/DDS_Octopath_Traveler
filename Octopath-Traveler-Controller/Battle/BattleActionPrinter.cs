using Octopath_Traveler_View;

namespace Octopath_Traveler.Battle;

public sealed class BattleActionPrinter
{
    private const string SeparatorLine = "----------------------------------------";

    private readonly View _view;

    public BattleActionPrinter(View view)
    {
        _view = view;
    }

    public void PrintTravelerBasicAttack(TravelerBasicAttack attack)
    {
        _view.WriteLine(SeparatorLine);
        _view.WriteLine($"{attack.AttackerName} ataca");
        _view.WriteLine($"{attack.TargetName} recibe {attack.Damage} de daño de tipo {attack.WeaponType}");
        _view.WriteLine($"{attack.TargetName} termina con HP:{attack.TargetCurrentHp}");
    }

    public void PrintBeastAttack(BeastAttack attack)
    {
        _view.WriteLine(SeparatorLine);
        _view.WriteLine($"{attack.AttackerName} usa Attack");
        _view.WriteLine($"{attack.TargetName} recibe {attack.Damage} de daño físico");
        _view.WriteLine($"{attack.TargetName} termina con HP:{attack.TargetCurrentHp}");
    }
}

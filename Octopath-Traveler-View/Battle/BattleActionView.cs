using Octopath_Traveler_Models.Battle;

namespace Octopath_Traveler_View.Battle;

public sealed class BattleActionView
{
    private const string SeparatorLine = "----------------------------------------";

    private readonly View _view;

    public BattleActionView(View view)
    {
        _view = view;
    }

    public void PrintTravelerBasicAttack(TravelerBasicAttack attack)
    {
        _view.WriteLine(SeparatorLine);
        _view.WriteLine($"{attack.AttackerName} ataca");
        string weaknessSuffix = attack.IsWeaknessHit ? " con debilidad" : string.Empty;
        _view.WriteLine($"{attack.TargetName} recibe {attack.Damage} de da\u00f1o de tipo {attack.WeaponType}{weaknessSuffix}");
        if (attack.EnteredBreakingPoint)
            _view.WriteLine($"{attack.TargetName} entra en Breaking Point");
        _view.WriteLine($"{attack.TargetName} termina con HP:{attack.TargetCurrentHp}");
    }

    public void PrintTravelerSkill(TravelerSkillAction action)
    {
        _view.WriteLine(SeparatorLine);
        _view.WriteLine($"{action.TravelerName} usa {action.SkillName}");
        foreach (string resultLine in action.ResultLines)
            _view.WriteLine(resultLine);
    }

    public void PrintBeastAttack(BeastAttack attack)
    {
        _view.WriteLine(SeparatorLine);
        _view.WriteLine($"{attack.AttackerName} usa {attack.SkillName}");
        foreach (string resultLine in attack.ResultLines)
            _view.WriteLine(resultLine);
    }
}

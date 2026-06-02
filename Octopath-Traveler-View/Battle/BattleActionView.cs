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
        foreach (TravelerBasicAttackHit hit in attack.Hits)
        {
            string weaknessSuffix = hit.IsWeaknessHit ? " con debilidad" : string.Empty;
            _view.WriteLine($"{attack.TargetName} recibe {hit.Damage} de da\u00f1o de tipo {attack.WeaponType}{weaknessSuffix}");
            if (hit.EnteredBreakingPoint)
                _view.WriteLine($"{attack.TargetName} entra en Breaking Point");
        }

        _view.WriteLine($"{attack.TargetName} termina con HP:{attack.TargetCurrentHp}");
    }

    public void PrintTravelerSkill(TravelerSkillAction action)
    {
        _view.WriteLine(SeparatorLine);
        _view.WriteLine($"{action.TravelerName} usa {action.SkillName}");
        foreach (TravelerSkillResult result in action.Results)
            PrintTravelerSkillResult(result);
    }

    private void PrintTravelerSkillResult(TravelerSkillResult result)
    {
        string resultLine = BuildTravelerSkillResultLine(result);
        _view.WriteLine(resultLine);
    }

    private static string BuildTravelerSkillResultLine(TravelerSkillResult result)
        => result switch
        {
            TravelerSkillDamageResult damageResult => BuildDamageLine(damageResult),
            TravelerSkillBreakingPointResult breakingPointResult =>
                $"{breakingPointResult.TargetName} entra en Breaking Point",
            TravelerSkillHpSummaryResult hpSummaryResult =>
                $"{hpSummaryResult.TargetName} termina con HP:{hpSummaryResult.CurrentHp}",
            TravelerSkillHealingResult healingResult =>
                $"{healingResult.TargetName} recupera {healingResult.HealedValue} de vida",
            TravelerSkillReviveResult reviveResult => $"{reviveResult.TargetName} revive",
            TravelerSkillPriorityChangeResult priorityChangeResult =>
                $"{priorityChangeResult.TargetName} tendr\u00e1 menor prioridad de turno durante {priorityChangeResult.DurationRounds} rondas",
            _ => throw new InvalidOperationException("Unsupported traveler skill result.")
        };

    private static string BuildDamageLine(TravelerSkillDamageResult damageResult)
    {
        string weaknessSuffix = damageResult.IsWeaknessHit ? " con debilidad" : string.Empty;
        return $"{damageResult.TargetName} recibe {damageResult.Damage} de da\u00f1o de tipo {damageResult.DamageType}{weaknessSuffix}";
    }

    public void PrintBeastAttack(BeastAttack attack)
    {
        _view.WriteLine(SeparatorLine);
        _view.WriteLine($"{attack.AttackerName} usa {attack.SkillName}");
        foreach (BeastAttackResult result in attack.Results)
            PrintBeastAttackResult(result);
    }

    private void PrintBeastAttackResult(BeastAttackResult result)
    {
        string resultLine = BuildBeastAttackResultLine(result);
        _view.WriteLine(resultLine);
    }

    private static string BuildBeastAttackResultLine(BeastAttackResult result)
        => result switch
        {
            BeastAttackDefendResult defendResult => $"{defendResult.TargetName} se defiende",
            BeastAttackDamageResult damageResult => BuildBeastDamageLine(damageResult),
            BeastAttackHpSummaryResult hpSummaryResult =>
                $"{hpSummaryResult.TargetName} termina con HP:{hpSummaryResult.CurrentHp}",
            _ => throw new InvalidOperationException("Unsupported beast attack result.")
        };

    private static string BuildBeastDamageLine(BeastAttackDamageResult damageResult)
        => damageResult.DamageKind switch
        {
            BeastAttackDamageKind.Physical =>
                $"{damageResult.TargetName} recibe {damageResult.Damage} de da\u00f1o f\u00edsico",
            BeastAttackDamageKind.Elemental =>
                $"{damageResult.TargetName} recibe {damageResult.Damage} de da\u00f1o elemental",
            _ => $"{damageResult.TargetName} recibe {damageResult.Damage} de da\u00f1o"
        };

    public void PrintPatienceExtraTurn(string travelerName)
    {
        _view.WriteLine(SeparatorLine);
        _view.WriteLine($"{travelerName} obtiene un turno adicional");
    }
}

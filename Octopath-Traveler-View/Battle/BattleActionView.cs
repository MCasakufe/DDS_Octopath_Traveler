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

    public void WriteTravelerBasicAttack(TravelerBasicAttack attack)
    {
        _view.WriteLine(SeparatorLine);
        _view.WriteLine($"{attack.AttackerName} ataca");
        WriteTravelerBasicAttackHits(attack);
        WriteBasicAttackSpRecovery(attack.SpRecoveryResult);
        _view.WriteLine($"{attack.TargetName} termina con HP:{attack.TargetCurrentHp}");
    }

    private void WriteTravelerBasicAttackHits(TravelerBasicAttack attack)
    {
        foreach (TravelerBasicAttackHit hit in attack.Hits)
            WriteTravelerBasicAttackHit(attack, hit);
    }

    private void WriteTravelerBasicAttackHit(TravelerBasicAttack attack, TravelerBasicAttackHit hit)
    {
        _view.WriteLine(BuildTravelerBasicAttackDamageLine(attack, hit));
        if (hit.EnteredBreakingPoint)
            _view.WriteLine($"{attack.TargetName} entra en Breaking Point");
    }

    private static string BuildTravelerBasicAttackDamageLine(
        TravelerBasicAttack attack,
        TravelerBasicAttackHit hit)
    {
        string weaknessSuffix = hit.IsWeaknessHit ? " con debilidad" : string.Empty;
        return $"{attack.TargetName} recibe {hit.Damage} de da\u00f1o de tipo {attack.WeaponType}{weaknessSuffix}";
    }

    private void WriteBasicAttackSpRecovery(TravelerBasicAttackSpRecoveryResult? spRecoveryResult)
    {
        if (spRecoveryResult is not null)
            _view.WriteLine($"{spRecoveryResult.TargetName} recupera {spRecoveryResult.RecoveredSp} SP");
    }

    public void WriteTravelerSkill(TravelerSkillAction action)
    {
        _view.WriteLine(SeparatorLine);
        _view.WriteLine($"{action.TravelerName} usa {action.SkillName}");
        foreach (TravelerSkillResult result in action.Results)
            WriteTravelerSkillResult(result);
    }

    private void WriteTravelerSkillResult(TravelerSkillResult result)
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
            TravelerSkillSpRecoveryResult spRecoveryResult =>
                $"{spRecoveryResult.TargetName} recupera {spRecoveryResult.RecoveredSp} SP",
            TravelerSkillReviveResult reviveResult => $"{reviveResult.TargetName} revive",
            TravelerSkillPriorityChangeResult priorityChangeResult => BuildPriorityChangeLine(priorityChangeResult),
            TravelerSkillHpRestorationStatusResult hpRestorationStatusResult =>
                BuildHpRestorationStatusLine(hpRestorationStatusResult),
            TravelerSkillActivationDuplicationStatusResult activationDuplicationStatusResult =>
                BuildActivationDuplicationStatusLine(activationDuplicationStatusResult),
            TravelerSkillTargetModificationStatusResult targetModificationStatusResult =>
                BuildTargetModificationStatusLine(targetModificationStatusResult),
            TravelerSkillStatusEffectResult statusEffectResult =>
                BuildStatusEffectLine(
                    statusEffectResult.TargetName,
                    statusEffectResult.StatusEffectKind,
                    statusEffectResult.DurationRounds),
            _ => throw new InvalidOperationException("Unsupported traveler skill result.")
        };

    private static string BuildDamageLine(TravelerSkillDamageResult damageResult)
    {
        string weaknessSuffix = damageResult.IsWeaknessHit ? " con debilidad" : string.Empty;
        return $"{damageResult.TargetName} recibe {damageResult.Damage} de da\u00f1o de tipo "
               + $"{damageResult.DamageType}{weaknessSuffix}";
    }

    private static string BuildPriorityChangeLine(TravelerSkillPriorityChangeResult priorityChangeResult)
        => $"{priorityChangeResult.TargetName} tendr\u00e1 menor prioridad de turno durante "
           + $"{priorityChangeResult.DurationRounds} rondas";

    private static string BuildHpRestorationStatusLine(
        TravelerSkillHpRestorationStatusResult hpRestorationStatusResult)
        => $"{hpRestorationStatusResult.TargetName} tendr\u00e1 restauraci\u00f3n de HP durante "
           + $"{hpRestorationStatusResult.DurationRounds} rondas";

    private static string BuildActivationDuplicationStatusLine(
        TravelerSkillActivationDuplicationStatusResult activationDuplicationStatusResult)
        => $"{activationDuplicationStatusResult.TargetName} activar\u00e1 sus habilidades 2 veces durante "
           + $"{activationDuplicationStatusResult.DurationRounds} rondas";

    private static string BuildTargetModificationStatusLine(
        TravelerSkillTargetModificationStatusResult targetModificationStatusResult)
        => $"{targetModificationStatusResult.TargetName} modificar\u00e1 sus targets durante "
           + $"{targetModificationStatusResult.DurationRounds} rondas";

    public void WriteBeastAttack(BeastAttack attack)
    {
        _view.WriteLine(SeparatorLine);
        _view.WriteLine($"{attack.AttackerName} usa {attack.SkillName}");
        foreach (BeastAttackResult result in attack.Results)
            WriteBeastAttackResult(result);
    }

    private void WriteBeastAttackResult(BeastAttackResult result)
    {
        string resultLine = BuildBeastAttackResultLine(result);
        _view.WriteLine(resultLine);
    }

    private static string BuildBeastAttackResultLine(BeastAttackResult result)
        => result switch
        {
            BeastAttackDefendResult defendResult => $"{defendResult.TargetName} se defiende",
            BeastAttackDamageResult damageResult => BuildBeastDamageLine(damageResult),
            BeastAttackReviveResult reviveResult => $"{reviveResult.TargetName} revive",
            BeastAttackHpSummaryResult hpSummaryResult =>
                $"{hpSummaryResult.TargetName} termina con HP:{hpSummaryResult.CurrentHp}",
            BeastAttackStatusEffectResult statusEffectResult =>
                BuildStatusEffectLine(
                    statusEffectResult.TargetName,
                    statusEffectResult.StatusEffectKind,
                    statusEffectResult.DurationRounds),
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

    private static string BuildStatusEffectLine(
        string targetName,
        UnitStatusEffectKind statusEffectKind,
        int durationRounds)
        => $"{targetName} tendr\u00e1 {FormatStatusEffect(statusEffectKind)} durante {durationRounds} rondas";

    private static string FormatStatusEffect(UnitStatusEffectKind statusEffectKind)
        => statusEffectKind switch
        {
            UnitStatusEffectKind.IncreasedPhysicalAttack => "Increased Physical Attack",
            UnitStatusEffectKind.IncreasedPhysicalDefense => "Increased Physical Defense",
            UnitStatusEffectKind.IncreasedElementalAttack => "Increased Elemental Attack",
            UnitStatusEffectKind.IncreasedElementalDefense => "Increased Elemental Defense",
            UnitStatusEffectKind.IncreasedSpeed => "Increased Speed",
            UnitStatusEffectKind.DecreasedPhysicalAttack => "Decreased Physical Attack",
            UnitStatusEffectKind.DecreasedPhysicalDefense => "Decreased Physical Defense",
            UnitStatusEffectKind.DecreasedElementalDefense => "Decreased Elemental Defense",
            _ => throw new InvalidOperationException("Unsupported status effect.")
        };

    public void WritePatienceExtraTurn(string travelerName)
    {
        _view.WriteLine(SeparatorLine);
        _view.WriteLine($"{travelerName} obtiene un turno adicional");
    }
}

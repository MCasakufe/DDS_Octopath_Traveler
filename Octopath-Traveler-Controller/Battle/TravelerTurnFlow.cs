using Octopath_Traveler_View;

namespace Octopath_Traveler.Battle;

public enum TravelerTurnResolution
{
    None,
    BasicAttackChosen,
    SkillChosen,
    DefendChosen,
    Fled
}

public sealed record TravelerTurnOutcome(
    TravelerTurnResolution Resolution,
    string? SelectedWeapon,
    BeastCombatUnit? SelectedTarget,
    int UsedBp)
{
    public static TravelerTurnOutcome NoAction()
        => new(TravelerTurnResolution.None, null, null, 0);

    public static TravelerTurnOutcome BasicAttack(string selectedWeapon, BeastCombatUnit selectedTarget, int usedBp)
        => new(TravelerTurnResolution.BasicAttackChosen, selectedWeapon, selectedTarget, usedBp);

    public static TravelerTurnOutcome Skill()
        => new(TravelerTurnResolution.SkillChosen, null, null, 0);

    public static TravelerTurnOutcome Defend()
        => new(TravelerTurnResolution.DefendChosen, null, null, 0);

    public static TravelerTurnOutcome Flee()
        => new(TravelerTurnResolution.Fled, null, null, 0);
}

public sealed class TravelerTurnFlow
{
    private const string SeparatorLine = "----------------------------------------";

    private readonly View _view;

    public TravelerTurnFlow(View view)
    {
        _view = view;
    }

    public TravelerTurnOutcome RunTurn(TravelerCombatUnit traveler, BattleState battleState)
    {
        while (true)
        {
            WriteActionMenu(traveler.Name);
            var selectedAction = ReadMenuOption();

            if (selectedAction == 1)
            {
                var selectedWeapon = TrySelectWeapon(traveler);
                if (selectedWeapon is null)
                    continue;

                var selectedTarget = TrySelectTarget(traveler.Name, battleState);
                if (selectedTarget is null)
                    continue;

                var usedBp = ReadUsedBp(traveler.CurrentBp);
                return TravelerTurnOutcome.BasicAttack(selectedWeapon, selectedTarget, usedBp);
            }

            if (selectedAction == 2)
            {
                var selectedSkill = TrySelectSkill(traveler);
                if (selectedSkill is null)
                    continue;

                return TravelerTurnOutcome.Skill();
            }

            if (selectedAction == 3)
                return TravelerTurnOutcome.Defend();

            if (selectedAction == 4)
                return TravelerTurnOutcome.Flee();
        }
    }

    private void WriteActionMenu(string travelerName)
    {
        _view.WriteLine(SeparatorLine);
        _view.WriteLine($"Turno de {travelerName}");
        _view.WriteLine("1: Ataque básico");
        _view.WriteLine("2: Usar habilidad");
        _view.WriteLine("3: Defender");
        _view.WriteLine("4: Huir");
    }

    private string? TrySelectWeapon(TravelerCombatUnit traveler)
    {
        _view.WriteLine(SeparatorLine);
        _view.WriteLine("Seleccione un arma");

        for (var index = 0; index < traveler.Weapons.Count; index++)
            _view.WriteLine($"{index + 1}: {traveler.Weapons[index]}");

        var cancelOption = traveler.Weapons.Count + 1;
        _view.WriteLine($"{cancelOption}: Cancelar");

        var selectedWeaponOption = ReadMenuOption();
        if (selectedWeaponOption == cancelOption)
            return null;

        if (selectedWeaponOption is < 1 || selectedWeaponOption > traveler.Weapons.Count)
            return null;

        return traveler.Weapons[selectedWeaponOption.Value - 1];
    }

    private BeastCombatUnit? TrySelectTarget(string travelerName, BattleState battleState)
    {
        var aliveBeasts = battleState.BeastTeam.Where(beast => beast.IsAlive).ToList();

        _view.WriteLine(SeparatorLine);
        _view.WriteLine($"Seleccione un objetivo para {travelerName}");

        for (var index = 0; index < aliveBeasts.Count; index++)
        {
            var beast = aliveBeasts[index];
            _view.WriteLine($"{index + 1}: {beast.Name} - HP:{beast.CurrentHp}/{beast.MaxHp} Shields:{beast.CurrentShields}");
        }

        var cancelOption = aliveBeasts.Count + 1;
        _view.WriteLine($"{cancelOption}: Cancelar");

        var selectedTargetOption = ReadMenuOption();
        if (selectedTargetOption == cancelOption)
            return null;

        if (selectedTargetOption is < 1 || selectedTargetOption > aliveBeasts.Count)
            return null;

        return aliveBeasts[selectedTargetOption.Value - 1];
    }

    private string? TrySelectSkill(TravelerCombatUnit traveler)
    {
        _view.WriteLine(SeparatorLine);
        _view.WriteLine($"Seleccione una habilidad para {traveler.Name}");

        for (var index = 0; index < traveler.AssignedActiveSkillNames.Count; index++)
            _view.WriteLine($"{index + 1}: {traveler.AssignedActiveSkillNames[index]}");

        var cancelOption = traveler.AssignedActiveSkillNames.Count + 1;
        _view.WriteLine($"{cancelOption}: Cancelar");

        var selectedSkillOption = ReadMenuOption();
        if (selectedSkillOption == cancelOption)
            return null;

        if (selectedSkillOption is < 1 || selectedSkillOption > traveler.AssignedActiveSkillNames.Count)
            return null;

        return traveler.AssignedActiveSkillNames[selectedSkillOption.Value - 1];
    }

    private int ReadUsedBp(int currentBp)
    {
        if (currentBp < 1)
            return 0;

        _view.WriteLine(SeparatorLine);
        _view.WriteLine("Seleccione cuantos BP utilizar");
        _view.ReadLine();
        return 0;
    }

    private int? ReadMenuOption()
    {
        var optionText = _view.ReadLine();
        return int.TryParse(optionText, out var option) ? option : null;
    }
}
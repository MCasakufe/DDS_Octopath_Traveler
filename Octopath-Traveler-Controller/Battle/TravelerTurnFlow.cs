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

    private enum TravelerActionOption
    {
        BasicAttack = 1,
        Skill = 2,
        Defend = 3,
        Flee = 4
    }

    public TravelerTurnFlow(View view)
    {
        _view = view;
    }

    public TravelerTurnOutcome RunTurn(TravelerCombatUnit traveler, BattleState battleState)
    {
        while (true)
        {
            WriteActionMenu(traveler.Name);
            var selectedAction = TryReadActionOption();
            var turnOutcome = TryCreateOutcome(selectedAction, traveler, battleState);
            if (turnOutcome is not null)
                return turnOutcome;
        }
    }

    private TravelerTurnOutcome? TryCreateOutcome(
        TravelerActionOption? selectedAction,
        TravelerCombatUnit traveler,
        BattleState battleState)
        => selectedAction switch
        {
            TravelerActionOption.BasicAttack => TryCreateBasicAttackOutcome(traveler, battleState),
            TravelerActionOption.Skill => TryCreateSkillOutcome(traveler),
            TravelerActionOption.Defend => TravelerTurnOutcome.Defend(),
            TravelerActionOption.Flee => TravelerTurnOutcome.Flee(),
            _ => null
        };

    private TravelerTurnOutcome? TryCreateBasicAttackOutcome(TravelerCombatUnit traveler, BattleState battleState)
    {
        var selectedWeapon = TrySelectWeapon(traveler);
        if (selectedWeapon is null)
            return null;

        var selectedTarget = TrySelectTarget(traveler.Name, battleState);
        if (selectedTarget is null)
            return null;

        var usedBp = ReadUsedBp(traveler.CurrentBp);
        return TravelerTurnOutcome.BasicAttack(selectedWeapon, selectedTarget, usedBp);
    }

    private TravelerTurnOutcome? TryCreateSkillOutcome(TravelerCombatUnit traveler)
    {
        var selectedSkill = TrySelectSkill(traveler);
        return selectedSkill is null
            ? null
            : TravelerTurnOutcome.Skill();
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
        if (selectedWeaponOption is not int selectedWeaponIndex)
            return null;

        if (selectedWeaponOption == cancelOption)
            return null;

        if (selectedWeaponIndex < 1 || selectedWeaponIndex > traveler.Weapons.Count)
            return null;

        return traveler.Weapons[selectedWeaponIndex - 1];
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
        if (selectedTargetOption is not int selectedTargetIndex)
            return null;

        if (selectedTargetOption == cancelOption)
            return null;

        if (selectedTargetIndex < 1 || selectedTargetIndex > aliveBeasts.Count)
            return null;

        return aliveBeasts[selectedTargetIndex - 1];
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
        if (selectedSkillOption is not int selectedSkillIndex)
            return null;

        if (selectedSkillOption == cancelOption)
            return null;

        if (selectedSkillIndex < 1 || selectedSkillIndex > traveler.AssignedActiveSkillNames.Count)
            return null;

        return traveler.AssignedActiveSkillNames[selectedSkillIndex - 1];
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

    private TravelerActionOption? TryReadActionOption()
    {
        var option = ReadMenuOption();
        return option switch
        {
            (int)TravelerActionOption.BasicAttack => TravelerActionOption.BasicAttack,
            (int)TravelerActionOption.Skill => TravelerActionOption.Skill,
            (int)TravelerActionOption.Defend => TravelerActionOption.Defend,
            (int)TravelerActionOption.Flee => TravelerActionOption.Flee,
            _ => null
        };
    }
}

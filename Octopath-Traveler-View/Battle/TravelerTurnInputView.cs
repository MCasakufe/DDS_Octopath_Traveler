using Octopath_Traveler_Models.Battle;

namespace Octopath_Traveler_View.Battle;

public sealed class TravelerTurnInputView
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

    public TravelerTurnInputView(View view)
    {
        _view = view;
    }

    public TravelerTurnOutcome RequestTurn(TravelerCombatUnit traveler, BattleState battleState)
    {
        while (true)
        {
            TravelerActionOption? selectedAction = SelectAction(traveler.Name);
            TravelerTurnOutcome? turnOutcome = CreateTurnOutcome(selectedAction, traveler, battleState);
            if (turnOutcome is not null)
                return turnOutcome;
        }
    }

    private TravelerActionOption? SelectAction(string travelerName)
    {
        WriteActionMenu(travelerName);
        return ReadActionOption();
    }

    private TravelerTurnOutcome? CreateTurnOutcome(
        TravelerActionOption? selectedAction,
        TravelerCombatUnit traveler,
        BattleState battleState)
        => selectedAction switch
        {
            TravelerActionOption.BasicAttack => CreateBasicAttackOutcome(traveler, battleState),
            TravelerActionOption.Skill => CreateSkillOutcome(traveler),
            TravelerActionOption.Defend => TravelerTurnOutcome.Defend(),
            TravelerActionOption.Flee => TravelerTurnOutcome.Flee(),
            _ => null
        };

    private TravelerTurnOutcome? CreateBasicAttackOutcome(TravelerCombatUnit traveler, BattleState battleState)
    {
        BasicAttackSelection? basicAttackSelection = TryCreateBasicAttackSelection(traveler, battleState);
        if (basicAttackSelection is null)
            return null;

        return TravelerTurnOutcome.BasicAttack(
            basicAttackSelection.SelectedWeapon,
            basicAttackSelection.SelectedTarget,
            basicAttackSelection.UsedBp);
    }

    private TravelerTurnOutcome? CreateSkillOutcome(TravelerCombatUnit traveler)
    {
        string? selectedSkill = SelectSkill(traveler);
        return selectedSkill is null ? null : TravelerTurnOutcome.Skill();
    }

    private BasicAttackSelection? TryCreateBasicAttackSelection(TravelerCombatUnit traveler, BattleState battleState)
    {
        string? selectedWeapon = SelectWeapon(traveler);
        if (selectedWeapon is null)
            return null;

        BeastCombatUnit? selectedTarget = SelectTarget(traveler.Name, battleState);
        if (selectedTarget is null)
            return null;

        int usedBp = ReadUsedBp(traveler.CurrentBp);
        return new BasicAttackSelection(selectedWeapon, selectedTarget, usedBp);
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

    private string? SelectWeapon(TravelerCombatUnit traveler)
    {
        WriteMenu("Seleccione un arma", traveler.Weapons);
        int? selectedIndex = ReadSelectedIndex(traveler.Weapons.Count);
        return selectedIndex is null ? null : traveler.Weapons[selectedIndex.Value];
    }

    private BeastCombatUnit? SelectTarget(string travelerName, BattleState battleState)
    {
        List<BeastCombatUnit> aliveBeasts = battleState.BeastTeam.Where(beast => beast.IsAlive).ToList();
        List<string> targetOptions = BuildTargetOptions(aliveBeasts);

        WriteMenu($"Seleccione un objetivo para {travelerName}", targetOptions);
        int? selectedIndex = ReadSelectedIndex(aliveBeasts.Count);
        return selectedIndex is null ? null : aliveBeasts[selectedIndex.Value];
    }

    private static List<string> BuildTargetOptions(IEnumerable<BeastCombatUnit> aliveBeasts)
        => aliveBeasts
            .Select(beast => $"{beast.Name} - HP:{beast.CurrentHp}/{beast.MaxHp} Shields:{beast.CurrentShields}")
            .ToList();

    private string? SelectSkill(TravelerCombatUnit traveler)
    {
        WriteMenu($"Seleccione una habilidad para {traveler.Name}", traveler.AssignedActiveSkillNames);
        int? selectedIndex = ReadSelectedIndex(traveler.AssignedActiveSkillNames.Count);
        return selectedIndex is null ? null : traveler.AssignedActiveSkillNames[selectedIndex.Value];
    }

    private void WriteMenu(string title, IReadOnlyList<string> options)
    {
        _view.WriteLine(SeparatorLine);
        _view.WriteLine(title);

        for (int index = 0; index < options.Count; index++)
            _view.WriteLine($"{index + 1}: {options[index]}");

        _view.WriteLine($"{options.Count + 1}: Cancelar");
    }

    private int? ReadSelectedIndex(int selectableOptionCount)
    {
        int? selectedOption = ReadMenuOption();
        if (selectedOption is null)
            return null;

        int cancelOption = selectableOptionCount + 1;
        if (selectedOption.Value == cancelOption)
            return null;

        int selectedIndex = selectedOption.Value - 1;
        return selectedIndex >= 0 && selectedIndex < selectableOptionCount
            ? selectedIndex
            : null;
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
        string? optionText = _view.ReadLine();
        return int.TryParse(optionText, out var option) ? option : null;
    }

    private TravelerActionOption? ReadActionOption()
    {
        int? option = ReadMenuOption();
        return option switch
        {
            (int)TravelerActionOption.BasicAttack => TravelerActionOption.BasicAttack,
            (int)TravelerActionOption.Skill => TravelerActionOption.Skill,
            (int)TravelerActionOption.Defend => TravelerActionOption.Defend,
            (int)TravelerActionOption.Flee => TravelerActionOption.Flee,
            _ => null
        };
    }

    private sealed record BasicAttackSelection(string SelectedWeapon, BeastCombatUnit SelectedTarget, int UsedBp);
}

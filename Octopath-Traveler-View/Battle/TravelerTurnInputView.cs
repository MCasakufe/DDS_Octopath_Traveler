using Octopath_Traveler_Models.Battle;
using Octopath_Traveler_Models.RuntimeData;

namespace Octopath_Traveler_View.Battle;

public sealed class TravelerTurnInputView
{
    private const string SeparatorLine = "----------------------------------------";
    private const int MenuOptionNumberOffset = 1;
    private const int MinimumSelectableIndex = 0;
    private const int MinimumBpToPrompt = 1;
    private const int DefaultUsedBp = 0;
    private const int MinimumRequestedBp = 0;
    private const int MaximumUsedBp = 3;

    private readonly View _view;
    private readonly TravelerTurnSelectionPolicy _selectionPolicy;
    private readonly TravelerTurnOutcomeFactory _outcomeFactory;

    public TravelerTurnInputView(View view)
    {
        _view = view;
        _selectionPolicy = new TravelerTurnSelectionPolicy();
        _outcomeFactory = new TravelerTurnOutcomeFactory();
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
    {
        TravelerTurnOutcome? immediateOutcome = _outcomeFactory.CreateImmediateOutcome(selectedAction);
        if (immediateOutcome is not null)
            return immediateOutcome;

        return selectedAction switch
        {
            TravelerActionOption.BasicAttack => CreateBasicAttackOutcome(traveler, battleState),
            TravelerActionOption.Skill => CreateSkillOutcome(traveler, battleState),
            _ => null
        };
    }

    private TravelerTurnOutcome? CreateBasicAttackOutcome(TravelerCombatUnit traveler, BattleState battleState)
    {
        BasicAttackSelection? basicAttackSelection = TryCreateBasicAttackSelection(traveler, battleState);
        if (basicAttackSelection is null)
            return null;

        return _outcomeFactory.CreateBasicAttackOutcome(basicAttackSelection);
    }

    private TravelerTurnOutcome? CreateSkillOutcome(TravelerCombatUnit traveler, BattleState battleState)
    {
        SkillDefinition? selectedSkill = SelectSkill(traveler);
        if (selectedSkill is null)
            return null;

        TravelerSkillSelection? skillSelection = TryCreateSkillSelection(selectedSkill, traveler, battleState);
        if (skillSelection is null)
            return null;

        return _outcomeFactory.CreateSkillOutcome(skillSelection);
    }

    private BasicAttackSelection? TryCreateBasicAttackSelection(TravelerCombatUnit traveler, BattleState battleState)
    {
        string? selectedWeapon = SelectWeapon(traveler.Weapons);
        if (selectedWeapon is null)
            return null;

        BeastCombatUnit? selectedTarget = SelectBeastTarget(
            traveler.Name,
            _selectionPolicy.SelectBeastTargets(battleState));
        if (selectedTarget is null)
            return null;

        int usedBp = ReadUsedBp(traveler.Name, traveler.CurrentBp);
        return new BasicAttackSelection(selectedWeapon, selectedTarget, usedBp);
    }

    private TravelerSkillSelection? TryCreateSkillSelection(
        SkillDefinition selectedSkill,
        TravelerCombatUnit traveler,
        BattleState battleState)
    {
        TravelerSkillInputPlan inputPlan = _selectionPolicy.CreateSkillInputPlan(
            selectedSkill,
            traveler,
            battleState);
        string? selectedWeapon = TrySelectSkillWeapon(inputPlan);
        if (inputPlan.RequiresWeaponSelection && selectedWeapon is null)
            return null;

        TravelerSkillTargetSelection? selectedTarget =
            TrySelectSkillTarget(traveler.Name, battleState, inputPlan);
        if (selectedTarget is null)
            return null;

        int usedBp = SelectSkillUsedBp(selectedSkill, traveler);
        return new TravelerSkillSelection(
            selectedSkill,
            selectedTarget.SelectedBeastTarget,
            selectedTarget.SelectedTravelerTarget,
            selectedWeapon,
            usedBp);
    }

    private int SelectSkillUsedBp(SkillDefinition selectedSkill, TravelerCombatUnit traveler)
    {
        if (!_selectionPolicy.RequiresBpSelection(selectedSkill))
            return _selectionPolicy.SelectAutomaticBpCost(selectedSkill);

        return ReadUsedBp(traveler.Name, traveler.CurrentBp);
    }

    private string? TrySelectSkillWeapon(TravelerSkillInputPlan inputPlan)
        => inputPlan.RequiresWeaponSelection ? SelectWeapon(inputPlan.SelectableWeaponTypes) : null;

    private TravelerSkillTargetSelection? TrySelectSkillTarget(
        string travelerName,
        BattleState battleState,
        TravelerSkillInputPlan inputPlan)
    {
        return inputPlan.TargetInputKind switch
        {
            TravelerSkillTargetInputKind.Beast => TrySelectBeastSkillTarget(travelerName, battleState),
            TravelerSkillTargetInputKind.Traveler => TrySelectTravelerSkillTarget(travelerName, inputPlan),
            _ => TravelerSkillTargetSelection.Empty
        };
    }

    private TravelerSkillTargetSelection? TrySelectBeastSkillTarget(
        string travelerName,
        BattleState battleState)
    {
        BeastCombatUnit? selectedTarget = SelectBeastTarget(
            travelerName,
            _selectionPolicy.SelectBeastTargets(battleState));
        return selectedTarget is null ? null : new TravelerSkillTargetSelection(selectedTarget, null);
    }

    private TravelerSkillTargetSelection? TrySelectTravelerSkillTarget(
        string travelerName,
        TravelerSkillInputPlan inputPlan)
    {
        TravelerCombatUnit? selectedTarget = SelectTravelerTarget(travelerName, inputPlan.SelectableTravelerTargets);
        return selectedTarget is null ? null : new TravelerSkillTargetSelection(null, selectedTarget);
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

    private string? SelectWeapon(IReadOnlyList<string> selectableWeapons)
    {
        WriteMenu("Seleccione un arma", selectableWeapons);
        int? selectedIndex = TryReadSelectedIndex(selectableWeapons.Count);
        return selectedIndex is null ? null : selectableWeapons[selectedIndex.Value];
    }

    private BeastCombatUnit? SelectBeastTarget(
        string travelerName,
        IReadOnlyList<BeastCombatUnit> selectableBeasts)
    {
        List<string> targetOptions = BuildBeastTargetOptions(selectableBeasts);

        WriteMenu($"Seleccione un objetivo para {travelerName}", targetOptions);
        int? selectedIndex = TryReadSelectedIndex(selectableBeasts.Count);
        return selectedIndex is null ? null : selectableBeasts[selectedIndex.Value];
    }

    private TravelerCombatUnit? SelectTravelerTarget(string travelerName, IReadOnlyList<TravelerCombatUnit> selectableTravelers)
    {
        List<string> targetOptions = BuildTravelerTargetOptions(selectableTravelers);
        WriteMenu($"Seleccione un objetivo para {travelerName}", targetOptions);
        int? selectedIndex = TryReadSelectedIndex(selectableTravelers.Count);
        return selectedIndex is null ? null : selectableTravelers[selectedIndex.Value];
    }

    private static List<string> BuildBeastTargetOptions(IEnumerable<BeastCombatUnit> aliveBeasts)
        => aliveBeasts
            .Select(beast => $"{beast.Name} - HP:{beast.CurrentHp}/{beast.MaxHp} Shields:{beast.CurrentShields}")
            .ToList();

    private static List<string> BuildTravelerTargetOptions(IEnumerable<TravelerCombatUnit> selectableTravelers)
        => selectableTravelers
            .Select(traveler =>
                $"{traveler.Name} - HP:{traveler.CurrentHp}/{traveler.MaxHp} SP:{traveler.CurrentSp}/{traveler.MaxSp} BP:{traveler.CurrentBp}")
            .ToList();

    private SkillDefinition? SelectSkill(TravelerCombatUnit traveler)
    {
        IReadOnlyList<SkillDefinition> castableSkills = _selectionPolicy.SelectCastableSkills(traveler);
        List<string> options = castableSkills.Select(skill => skill.Name).ToList();
        WriteMenu($"Seleccione una habilidad para {traveler.Name}", options);

        int? selectedIndex = TryReadSelectedIndex(castableSkills.Count);
        return selectedIndex is null ? null : castableSkills[selectedIndex.Value];
    }

    private void WriteMenu(string title, IReadOnlyList<string> options)
    {
        _view.WriteLine(SeparatorLine);
        _view.WriteLine(title);

        for (int index = 0; index < options.Count; index++)
            _view.WriteLine($"{index + MenuOptionNumberOffset}: {options[index]}");

        _view.WriteLine($"{options.Count + MenuOptionNumberOffset}: Cancelar");
    }

    private int? TryReadSelectedIndex(int selectableOptionCount)
    {
        int? selectedOption = TryReadMenuOption();
        if (selectedOption is null)
            return null;

        int cancelOption = selectableOptionCount + MenuOptionNumberOffset;
        if (selectedOption.Value == cancelOption)
            return null;

        int selectedIndex = selectedOption.Value - MenuOptionNumberOffset;
        return selectedIndex >= MinimumSelectableIndex && selectedIndex < selectableOptionCount
            ? selectedIndex
            : null;
    }

    private int ReadUsedBp(string travelerName, int currentBp)
    {
        if (currentBp < MinimumBpToPrompt)
            return DefaultUsedBp;

        while (true)
        {
            _view.WriteLine(SeparatorLine);
            _view.WriteLine("Seleccione cuantos BP utilizar");

            string? enteredText = _view.ReadLine();
            if (!int.TryParse(enteredText, out int requestedBp))
                return DefaultUsedBp;

            if (requestedBp < MinimumRequestedBp)
                return DefaultUsedBp;

            if (requestedBp > MaximumUsedBp || requestedBp > currentBp)
            {
                _view.WriteLine(SeparatorLine);
                _view.WriteLine($"{travelerName} no tiene {requestedBp} BP para utilizar");
                continue;
            }

            return requestedBp;
        }
    }

    private int? TryReadMenuOption()
    {
        string? optionText = _view.ReadLine();
        return int.TryParse(optionText, out int option) ? option : null;
    }

    private TravelerActionOption? ReadActionOption()
    {
        int? option = TryReadMenuOption();
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

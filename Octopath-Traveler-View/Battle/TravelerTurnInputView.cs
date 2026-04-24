using Octopath_Traveler_Models.Battle;
using Octopath_Traveler_Models.RuntimeData;

namespace Octopath_Traveler_View.Battle;

public sealed class TravelerTurnInputView
{
    private const string SeparatorLine = "----------------------------------------";
    private const string SingleTarget = "Single";
    private const string AllyTarget = "Ally";
    private static readonly IReadOnlyList<string> NightmareChimeraWeaponTypes =
    [
        "Sword",
        "Spear",
        "Dagger",
        "Axe",
        "Bow",
        "Stave"
    ];
    private static readonly IReadOnlySet<string> ReviveOnlyAllySkills = new HashSet<string>(StringComparer.Ordinal)
    {
        "Vivify"
    };

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
            TravelerActionOption.Skill => CreateSkillOutcome(traveler, battleState),
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

    private TravelerTurnOutcome? CreateSkillOutcome(TravelerCombatUnit traveler, BattleState battleState)
    {
        SkillDefinition? selectedSkill = SelectSkill(traveler);
        if (selectedSkill is null)
            return null;

        SkillSelection? skillSelection = TryCreateSkillSelection(selectedSkill, traveler, battleState);
        if (skillSelection is null)
            return null;

        int usedBp = ReadUsedBp(traveler.Name, traveler.CurrentBp);
        return TravelerTurnOutcome.Skill(
            selectedSkill.Name,
            skillSelection.SelectedBeastTarget,
            skillSelection.SelectedTravelerTarget,
            skillSelection.SelectedWeapon,
            usedBp);
    }

    private BasicAttackSelection? TryCreateBasicAttackSelection(TravelerCombatUnit traveler, BattleState battleState)
    {
        string? selectedWeapon = SelectWeapon(traveler.Weapons);
        if (selectedWeapon is null)
            return null;

        BeastCombatUnit? selectedTarget = SelectBeastTarget(traveler.Name, battleState);
        if (selectedTarget is null)
            return null;

        int usedBp = ReadUsedBp(traveler.Name, traveler.CurrentBp);
        return new BasicAttackSelection(selectedWeapon, selectedTarget, usedBp);
    }

    private SkillSelection? TryCreateSkillSelection(
        SkillDefinition selectedSkill,
        TravelerCombatUnit traveler,
        BattleState battleState)
    {
        string? selectedWeapon = null;
        if (selectedSkill.Name == "Nightmare Chimera")
        {
            selectedWeapon = SelectWeapon(NightmareChimeraWeaponTypes);
            if (selectedWeapon is null)
                return null;
        }

        if (selectedSkill.Target == SingleTarget)
        {
            BeastCombatUnit? selectedTarget = SelectBeastTarget(traveler.Name, battleState);
            return selectedTarget is null ? null : new SkillSelection(selectedTarget, null, selectedWeapon);
        }

        if (selectedSkill.Target == AllyTarget)
        {
            TravelerCombatUnit? selectedTarget = SelectTravelerTarget(
                traveler.Name,
                GetSelectableTravelerTargets(selectedSkill.Name, battleState));
            return selectedTarget is null ? null : new SkillSelection(null, selectedTarget, selectedWeapon);
        }

        return new SkillSelection(null, null, selectedWeapon);
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
        int? selectedIndex = ReadSelectedIndex(selectableWeapons.Count);
        return selectedIndex is null ? null : selectableWeapons[selectedIndex.Value];
    }

    private BeastCombatUnit? SelectBeastTarget(string travelerName, BattleState battleState)
    {
        List<BeastCombatUnit> aliveBeasts = battleState.BeastTeam.Where(beast => beast.IsAlive).ToList();
        List<string> targetOptions = BuildBeastTargetOptions(aliveBeasts);

        WriteMenu($"Seleccione un objetivo para {travelerName}", targetOptions);
        int? selectedIndex = ReadSelectedIndex(aliveBeasts.Count);
        return selectedIndex is null ? null : aliveBeasts[selectedIndex.Value];
    }

    private TravelerCombatUnit? SelectTravelerTarget(string travelerName, IReadOnlyList<TravelerCombatUnit> selectableTravelers)
    {
        List<string> targetOptions = BuildTravelerTargetOptions(selectableTravelers);
        WriteMenu($"Seleccione un objetivo para {travelerName}", targetOptions);
        int? selectedIndex = ReadSelectedIndex(selectableTravelers.Count);
        return selectedIndex is null ? null : selectableTravelers[selectedIndex.Value];
    }

    private static IReadOnlyList<TravelerCombatUnit> GetSelectableTravelerTargets(string skillName, BattleState battleState)
    {
        bool requiresDeadAllies = ReviveOnlyAllySkills.Contains(skillName);
        return battleState.TravelerTeam
            .Where(traveler => requiresDeadAllies ? !traveler.IsAlive : traveler.IsAlive)
            .ToList();
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
        List<SkillDefinition> castableSkills = traveler.AssignedActiveSkills
            .Where(skill => traveler.CurrentSp >= skill.Sp)
            .ToList();
        List<string> options = castableSkills.Select(skill => skill.Name).ToList();
        WriteMenu($"Seleccione una habilidad para {traveler.Name}", options);

        int? selectedIndex = ReadSelectedIndex(castableSkills.Count);
        return selectedIndex is null ? null : castableSkills[selectedIndex.Value];
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

    private int ReadUsedBp(string travelerName, int currentBp)
    {
        if (currentBp < 1)
            return 0;

        while (true)
        {
            _view.WriteLine(SeparatorLine);
            _view.WriteLine("Seleccione cuantos BP utilizar");

            string? enteredText = _view.ReadLine();
            if (!int.TryParse(enteredText, out int requestedBp))
                return 0;

            if (requestedBp < 0)
                return 0;

            if (requestedBp > 3 || requestedBp > currentBp)
            {
                _view.WriteLine(SeparatorLine);
                _view.WriteLine($"{travelerName} no tiene {requestedBp} BP para utilizar");
                continue;
            }

            return requestedBp;
        }
    }

    private int? ReadMenuOption()
    {
        string? optionText = _view.ReadLine();
        return int.TryParse(optionText, out int option) ? option : null;
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

    private sealed record SkillSelection(
        BeastCombatUnit? SelectedBeastTarget,
        TravelerCombatUnit? SelectedTravelerTarget,
        string? SelectedWeapon);
}

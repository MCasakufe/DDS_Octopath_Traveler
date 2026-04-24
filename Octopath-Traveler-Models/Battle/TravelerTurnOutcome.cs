namespace Octopath_Traveler_Models.Battle;

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
    BeastCombatUnit? SelectedBeastTarget,
    TravelerCombatUnit? SelectedTravelerTarget,
    string? SelectedSkillName,
    int UsedBp)
{
    public static TravelerTurnOutcome BasicAttack(string selectedWeapon, BeastCombatUnit selectedTarget, int usedBp)
        => new(TravelerTurnResolution.BasicAttackChosen, selectedWeapon, selectedTarget, null, null, usedBp);

    public static TravelerTurnOutcome Skill(
        string selectedSkillName,
        BeastCombatUnit? selectedBeastTarget,
        TravelerCombatUnit? selectedTravelerTarget,
        string? selectedWeapon,
        int usedBp)
        => new(
            TravelerTurnResolution.SkillChosen,
            selectedWeapon,
            selectedBeastTarget,
            selectedTravelerTarget,
            selectedSkillName,
            usedBp);

    public static TravelerTurnOutcome Defend()
        => new(TravelerTurnResolution.DefendChosen, null, null, null, null, 0);

    public static TravelerTurnOutcome Flee()
        => new(TravelerTurnResolution.Fled, null, null, null, null, 0);
}

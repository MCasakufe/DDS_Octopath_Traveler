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
    BeastCombatUnit? SelectedTarget,
    int UsedBp)
{
    public static TravelerTurnOutcome BasicAttack(string selectedWeapon, BeastCombatUnit selectedTarget, int usedBp)
        => new(TravelerTurnResolution.BasicAttackChosen, selectedWeapon, selectedTarget, usedBp);

    public static TravelerTurnOutcome Skill()
        => new(TravelerTurnResolution.SkillChosen, null, null, 0);

    public static TravelerTurnOutcome Defend()
        => new(TravelerTurnResolution.DefendChosen, null, null, 0);

    public static TravelerTurnOutcome Flee()
        => new(TravelerTurnResolution.Fled, null, null, 0);
}

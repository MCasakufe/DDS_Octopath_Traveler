namespace Octopath_Traveler_Models.Battle;

public sealed record TravelerBasicAttackExecutionRequest(
    TravelerCombatUnit Traveler,
    BeastCombatUnit Target,
    string WeaponType,
    int UsedBp);

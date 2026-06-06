namespace Octopath_Traveler_Models.Battle;

internal sealed record TravelerBasicAttackHitExecutionRequest(
    TravelerCombatUnit Traveler,
    BeastCombatUnit Target,
    string WeaponType);

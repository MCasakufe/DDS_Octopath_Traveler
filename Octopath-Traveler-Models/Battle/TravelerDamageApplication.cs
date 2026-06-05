namespace Octopath_Traveler_Models.Battle;

internal sealed record TravelerDamageApplication(
    Unit Attacker,
    TravelerCombatUnit Target,
    int Damage);

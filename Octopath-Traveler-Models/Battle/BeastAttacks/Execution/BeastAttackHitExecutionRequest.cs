namespace Octopath_Traveler_Models.Battle;

internal sealed record BeastAttackHitExecutionRequest(
    BeastCombatUnit Attacker,
    TravelerCombatUnit Target,
    BeastAttackDamageKind DamageKind);

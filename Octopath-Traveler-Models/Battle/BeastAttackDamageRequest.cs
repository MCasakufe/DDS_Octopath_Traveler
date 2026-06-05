namespace Octopath_Traveler_Models.Battle;

internal sealed record BeastAttackDamageRequest(
    BeastCombatUnit Attacker,
    TravelerCombatUnit Target,
    double SkillModifier,
    BeastAttackDamageKind DamageKind);

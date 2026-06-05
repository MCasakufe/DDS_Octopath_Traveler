namespace Octopath_Traveler_Models.Battle;

public sealed record BeastHitRequest(
    Unit Attacker,
    BeastCombatUnit Target,
    string DamageType,
    double SkillModifier);

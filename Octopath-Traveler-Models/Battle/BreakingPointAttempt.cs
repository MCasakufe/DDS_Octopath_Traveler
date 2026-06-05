namespace Octopath_Traveler_Models.Battle;

internal sealed record BreakingPointAttempt(
    BeastCombatUnit Target,
    bool IsWeaknessHit,
    int Damage,
    bool WasTargetInBreakingState);

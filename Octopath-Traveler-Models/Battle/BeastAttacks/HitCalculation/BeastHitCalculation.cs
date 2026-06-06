namespace Octopath_Traveler_Models.Battle;

internal sealed record BeastHitCalculation(
    int Damage,
    bool IsWeaknessHit,
    bool WasTargetInBreakingState);

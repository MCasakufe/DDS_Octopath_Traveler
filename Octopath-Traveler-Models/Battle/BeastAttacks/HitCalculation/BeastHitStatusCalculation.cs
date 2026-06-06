namespace Octopath_Traveler_Models.Battle;

internal sealed record BeastHitStatusCalculation(
    BeastHitRequest HitRequest,
    double BonusDamageMultiplier,
    HitStatus HitStatus);

namespace Octopath_Traveler_Models.Battle;

internal sealed record BeastHitCalculationRequest(
    BeastHitRequest HitRequest,
    double BonusDamageMultiplier,
    DamageCapType DamageCap);

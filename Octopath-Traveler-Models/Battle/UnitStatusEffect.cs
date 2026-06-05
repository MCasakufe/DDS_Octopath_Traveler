namespace Octopath_Traveler_Models.Battle;

public sealed record UnitStatusEffect(
    UnitStatusEffectKind Kind,
    int RemainingRounds);

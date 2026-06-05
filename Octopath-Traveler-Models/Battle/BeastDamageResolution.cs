namespace Octopath_Traveler_Models.Battle;

public sealed record BeastDamageResolution(
    int Damage,
    int TargetCurrentHp,
    bool IsWeaknessHit,
    bool EnteredBreakingPoint);

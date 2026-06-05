namespace Octopath_Traveler_Models.Battle;

public sealed record TravelerBasicAttackHit(
    int Damage,
    bool IsWeaknessHit,
    bool EnteredBreakingPoint);

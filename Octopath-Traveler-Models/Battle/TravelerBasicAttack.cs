namespace Octopath_Traveler_Models.Battle;

public sealed record TravelerBasicAttack(
    string AttackerName,
    string TargetName,
    string WeaponType,
    IReadOnlyList<TravelerBasicAttackHit> Hits,
    int TargetCurrentHp);

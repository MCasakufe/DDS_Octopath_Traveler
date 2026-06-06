namespace Octopath_Traveler_Models.Battle;

public sealed record TravelerSkillDamageResult(
    string TargetName,
    int Damage,
    string DamageType,
    bool IsWeaknessHit) : TravelerSkillResult;

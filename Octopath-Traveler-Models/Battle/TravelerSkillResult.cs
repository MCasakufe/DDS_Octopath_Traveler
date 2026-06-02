namespace Octopath_Traveler_Models.Battle;

public abstract record TravelerSkillResult;

public sealed record TravelerSkillDamageResult(
    string TargetName,
    int Damage,
    string DamageType,
    bool IsWeaknessHit) : TravelerSkillResult;

public sealed record TravelerSkillBreakingPointResult(string TargetName) : TravelerSkillResult;

public sealed record TravelerSkillHpSummaryResult(string TargetName, int CurrentHp) : TravelerSkillResult;

public sealed record TravelerSkillHealingResult(string TargetName, int HealedValue) : TravelerSkillResult;

public sealed record TravelerSkillReviveResult(string TargetName) : TravelerSkillResult;

public sealed record TravelerSkillPriorityChangeResult(
    string TargetName,
    int DurationRounds) : TravelerSkillResult;

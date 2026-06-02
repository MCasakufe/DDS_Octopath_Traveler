namespace Octopath_Traveler_Models.Battle;

public sealed record PassiveSkillProfile(
    PassiveStatBonus StatBonuses,
    bool HasBoostStart,
    bool HasStatSwap,
    bool HasVimAndVigor,
    bool HasSecondWind,
    bool HasPatience);

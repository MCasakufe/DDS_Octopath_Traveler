namespace Octopath_Traveler_Models.Battle;

public sealed record PassiveSkillProfile(
    PassiveStatBonus StatBonuses,
    bool HasBoostStart,
    bool HasStatSwap,
    bool HasVimAndVigor,
    bool HasSecondWind,
    bool HasPatience,
    bool HasPersistence,
    bool HasTheShowGoesOn,
    bool HasHangTough,
    bool HasSpSaver,
    bool HasEncore,
    bool HasInspiration,
    bool HasHeightenedHealing,
    bool HasDivineAura,
    bool HasSavingGrace,
    bool HasSecondServing,
    bool HasElementalEdge,
    bool HasPhysicalProwess,
    bool HasIntimidation);

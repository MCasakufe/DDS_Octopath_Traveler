namespace Octopath_Traveler_Models.Battle;

internal sealed class PassiveSkillProfileBuilder
{
    private readonly IReadOnlyDictionary<string, PassiveStatBonus> _passiveBonusesByName;
    private PassiveStatBonus _statBonuses = PassiveStatBonus.None;
    private bool _hasBoostStart;
    private bool _hasStatSwap;
    private bool _hasVimAndVigor;
    private bool _hasSecondWind;
    private bool _hasPatience;
    private bool _hasPersistence;
    private bool _hasTheShowGoesOn;
    private bool _hasHangTough;
    private bool _hasSpSaver;
    private bool _hasEncore;
    private bool _hasInspiration;
    private bool _hasHeightenedHealing;
    private bool _hasDivineAura;

    public PassiveSkillProfileBuilder(IReadOnlyDictionary<string, PassiveStatBonus> passiveBonusesByName)
    {
        _passiveBonusesByName = passiveBonusesByName;
    }

    public void ApplyPassiveSkill(string passiveSkillName)
    {
        AddStatBonus(passiveSkillName);
        _hasBoostStart = _hasBoostStart || passiveSkillName == "Boost Start";
        _hasStatSwap = _hasStatSwap || passiveSkillName == "Stat Swap";
        _hasVimAndVigor = _hasVimAndVigor || passiveSkillName == "Vim and Vigor";
        _hasSecondWind = _hasSecondWind || passiveSkillName == "Second Wind";
        _hasPatience = _hasPatience || passiveSkillName == "Patience";
        _hasPersistence = _hasPersistence || passiveSkillName == "Persistence";
        _hasTheShowGoesOn = _hasTheShowGoesOn || passiveSkillName == "The Show Goes On";
        _hasHangTough = _hasHangTough || passiveSkillName == "Hang Tough";
        _hasSpSaver = _hasSpSaver || passiveSkillName == "SP Saver";
        _hasEncore = _hasEncore || passiveSkillName == "Encore";
        _hasInspiration = _hasInspiration || passiveSkillName == "Inspiration";
        _hasHeightenedHealing = _hasHeightenedHealing || passiveSkillName == "Heightened Healing";
        _hasDivineAura = _hasDivineAura || passiveSkillName == "Divine Aura";
    }

    public PassiveSkillProfile Build()
        => new(
            _statBonuses,
            _hasBoostStart,
            _hasStatSwap,
            _hasVimAndVigor,
            _hasSecondWind,
            _hasPatience,
            _hasPersistence,
            _hasTheShowGoesOn,
            _hasHangTough,
            _hasSpSaver,
            _hasEncore,
            _hasInspiration,
            _hasHeightenedHealing,
            _hasDivineAura);

    private void AddStatBonus(string passiveSkillName)
    {
        if (_passiveBonusesByName.TryGetValue(passiveSkillName, out PassiveStatBonus passiveBonus))
            _statBonuses = _statBonuses.Add(passiveBonus);
    }
}

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
    }

    public PassiveSkillProfile Build()
        => new(_statBonuses, _hasBoostStart, _hasStatSwap, _hasVimAndVigor, _hasSecondWind, _hasPatience);

    private void AddStatBonus(string passiveSkillName)
    {
        if (_passiveBonusesByName.TryGetValue(passiveSkillName, out PassiveStatBonus passiveBonus))
            _statBonuses = _statBonuses.Add(passiveBonus);
    }
}

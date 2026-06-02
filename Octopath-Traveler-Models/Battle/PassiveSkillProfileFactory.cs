namespace Octopath_Traveler_Models.Battle;

public sealed class PassiveSkillProfileFactory
{
    private static readonly IReadOnlyDictionary<string, PassiveStatBonus> PassiveBonusesByName
        = new Dictionary<string, PassiveStatBonus>(StringComparer.Ordinal)
        {
            ["Elemental Augmentation"] = new(0, 0, 0, 50, 0),
            ["Summon Strength"] = new(0, 0, 50, 0, 0),
            ["Hale and Hearty"] = new(500, 0, 0, 0, 0),
            ["Fleefoot"] = new(0, 0, 0, 0, 50),
            ["Inner Strength"] = new(0, 50, 0, 0, 0)
        };

    public PassiveSkillProfile Create(IReadOnlyList<string> passiveSkillNames)
    {
        PassiveSkillProfileBuilder profileBuilder = new();
        foreach (string passiveSkillName in passiveSkillNames)
            profileBuilder.ApplyPassiveSkill(passiveSkillName);

        return profileBuilder.Build();
    }

    private sealed class PassiveSkillProfileBuilder
    {
        private PassiveStatBonus _statBonuses = PassiveStatBonus.None;
        private bool _hasBoostStart;
        private bool _hasStatSwap;
        private bool _hasVimAndVigor;
        private bool _hasSecondWind;
        private bool _hasPatience;

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
            if (PassiveBonusesByName.TryGetValue(passiveSkillName, out PassiveStatBonus passiveBonus))
                _statBonuses = _statBonuses.Add(passiveBonus);
        }
    }
}

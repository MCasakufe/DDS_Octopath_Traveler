namespace Octopath_Traveler_Models.Battle;

internal sealed class PassiveSkillNotifierFactory
{
    private readonly PassiveSkillProfileFactory _profileFactory = new();

    public PassiveSkillNotifier Create(IReadOnlyList<TravelerCombatUnit> travelers)
    {
        PassiveSkillNotifierBuilder notifierBuilder = new();
        foreach (TravelerCombatUnit traveler in travelers)
            notifierBuilder.RegisterTraveler(traveler, _profileFactory.Create(traveler.AssignedPassiveSkillNames));

        return notifierBuilder.Build();
    }

    private sealed class PassiveSkillNotifierBuilder
    {
        private readonly List<RoundEndPassiveSkillHandler> _roundEndHandlers = [];
        private readonly List<ExtraTurnPassiveSkillHandler> _extraTurnHandlers = [];

        public void RegisterTraveler(TravelerCombatUnit traveler, PassiveSkillProfile passiveSkillProfile)
        {
            AddRoundEndHandlers(traveler, passiveSkillProfile);
            AddExtraTurnHandlers(traveler, passiveSkillProfile);
        }

        public PassiveSkillNotifier Build()
            => new(_roundEndHandlers, _extraTurnHandlers);

        private void AddRoundEndHandlers(TravelerCombatUnit traveler, PassiveSkillProfile passiveSkillProfile)
        {
            if (passiveSkillProfile.HasVimAndVigor)
                _roundEndHandlers.Add(new VimAndVigorPassiveSkillHandler(traveler));

            if (passiveSkillProfile.HasSecondWind)
                _roundEndHandlers.Add(new SecondWindPassiveSkillHandler(traveler));
        }

        private void AddExtraTurnHandlers(TravelerCombatUnit traveler, PassiveSkillProfile passiveSkillProfile)
        {
            if (passiveSkillProfile.HasPatience)
                _extraTurnHandlers.Add(new PatiencePassiveSkillHandler(traveler));
        }
    }
}

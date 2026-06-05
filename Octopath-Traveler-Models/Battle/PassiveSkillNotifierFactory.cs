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
}

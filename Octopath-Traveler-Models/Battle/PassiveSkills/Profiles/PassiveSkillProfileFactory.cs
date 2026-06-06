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
        PassiveSkillProfileBuilder profileBuilder = new(PassiveBonusesByName);
        foreach (string passiveSkillName in passiveSkillNames)
            profileBuilder.ApplyPassiveSkill(passiveSkillName);

        return profileBuilder.Build();
    }
}

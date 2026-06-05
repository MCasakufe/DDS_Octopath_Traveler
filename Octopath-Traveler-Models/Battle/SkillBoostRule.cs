namespace Octopath_Traveler_Models.Battle;

internal sealed record SkillBoostRule(
    double FlatModifierIncrease,
    double PercentageModifierIncrease,
    int DurationIncreaseRounds)
{
    public static SkillBoostRule Unsupported { get; } = new(0, 0, 0);
}

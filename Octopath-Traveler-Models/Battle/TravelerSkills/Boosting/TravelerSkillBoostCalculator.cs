using System.Text.RegularExpressions;
using Octopath_Traveler_Models.RuntimeData;

namespace Octopath_Traveler_Models.Battle;

internal sealed class TravelerSkillBoostCalculator
{
    private const int MinimumUsedBp = 0;
    private const int PercentageDivisor = 100;
    private const double NoFlatModifierIncrease = 0;
    private const double NoPercentageModifierIncrease = 0;
    private const int NoDurationIncrease = 0;
    private const int ModifierPrecisionDigits = 10;

    private static readonly Regex NumberPattern = new(@"\d+(?:[.,]\d+)?", RegexOptions.Compiled);

    public double CalculateBoostedModifier(SkillDefinition skill, int usedBp)
    {
        SkillBoostRule boostRule = CreateBoostRule(skill);
        int normalizedUsedBp = NormalizeUsedBp(usedBp);
        double boostedModifier = skill.Modifier
                                  + boostRule.FlatModifierIncrease * normalizedUsedBp
                                  + skill.Modifier * boostRule.PercentageModifierIncrease * normalizedUsedBp;
        return Math.Round(boostedModifier, ModifierPrecisionDigits);
    }

    public int CalculateBoostedDuration(SkillDefinition skill, int baseDurationRounds, int usedBp)
    {
        SkillBoostRule boostRule = CreateBoostRule(skill);
        int normalizedUsedBp = NormalizeUsedBp(usedBp);
        return baseDurationRounds + boostRule.DurationIncreaseRounds * normalizedUsedBp;
    }

    private static SkillBoostRule CreateBoostRule(SkillDefinition skill)
    {
        if (IncreasesModifierByPercentage(skill.Boost))
            return PercentageModifierBoostRule(skill.Boost);

        if (IncreasesModifierByFlatValue(skill.Boost))
            return FlatModifierBoostRule(skill.Boost);

        if (IncreasesDuration(skill.Boost))
            return DurationBoostRule(skill.Boost);

        return SkillBoostRule.Unsupported;
    }

    private static bool IncreasesModifierByPercentage(string boostText)
        => boostText.Contains("modificador", StringComparison.OrdinalIgnoreCase)
           && boostText.Contains("%", StringComparison.Ordinal);

    private static bool IncreasesModifierByFlatValue(string boostText)
        => boostText.Contains("modificador", StringComparison.OrdinalIgnoreCase)
           && !boostText.Contains("%", StringComparison.Ordinal);

    private static bool IncreasesDuration(string boostText)
        => boostText.Contains("duraci", StringComparison.OrdinalIgnoreCase)
           && boostText.Contains("ronda", StringComparison.OrdinalIgnoreCase);

    private static SkillBoostRule PercentageModifierBoostRule(string boostText)
        => new(
            NoFlatModifierIncrease,
            ReadFirstNumber(boostText) / PercentageDivisor,
            NoDurationIncrease);

    private static SkillBoostRule FlatModifierBoostRule(string boostText)
        => new(ReadFirstNumber(boostText), NoPercentageModifierIncrease, NoDurationIncrease);

    private static SkillBoostRule DurationBoostRule(string boostText)
        => new(NoFlatModifierIncrease, NoPercentageModifierIncrease, (int)ReadFirstNumber(boostText));

    private static double ReadFirstNumber(string boostText)
    {
        Match numberMatch = NumberPattern.Match(boostText);
        if (!numberMatch.Success)
            return 0;

        string normalizedNumber = numberMatch.Value.Replace(',', '.');
        return double.Parse(normalizedNumber, System.Globalization.CultureInfo.InvariantCulture);
    }

    private static int NormalizeUsedBp(int usedBp)
        => Math.Max(MinimumUsedBp, usedBp);
}

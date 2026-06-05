using Octopath_Traveler_Models.RuntimeData;

namespace Octopath_Traveler_Models.Battle;

internal sealed class TravelerSkillHitCountResolver
{
    private const int DefaultHitCount = 1;

    private static readonly IReadOnlyDictionary<string, int> E3SkillHitCounts =
        new Dictionary<string, int>(StringComparer.Ordinal)
        {
            ["Fire Storm"] = 2,
            ["Blizzard"] = 2,
            ["Lightning Blast"] = 2,
            ["Ignis Ardere"] = 3,
            ["Glacies Claudere"] = 3,
            ["Tonitrus Canere"] = 3,
            ["Ventus Saltare"] = 3,
            ["Lux Congerere"] = 3,
            ["Tenebrae Operire"] = 3,
            ["Thousand Spears"] = 7,
            ["Rain of Arrows"] = 6,
            ["Arrowstorm"] = 6,
            ["Guardian Liondog"] = 5,
            ["HP Thief"] = 2,
            ["Steal SP"] = 2
        };

    public int ResolveHitCount(SkillDefinition skill)
    {
        if (skill.Hits > DefaultHitCount)
            return skill.Hits;

        return E3SkillHitCounts.TryGetValue(skill.Name, out int hitCount)
            ? hitCount
            : DefaultHitCount;
    }
}

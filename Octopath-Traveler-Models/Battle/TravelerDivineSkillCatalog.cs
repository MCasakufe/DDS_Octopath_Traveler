using Octopath_Traveler_Models.RuntimeData;

namespace Octopath_Traveler_Models.Battle;

public static class TravelerDivineSkillCatalog
{
    public const int RequiredBpCost = 3;
    public const string BrandsThunderName = "Brand's Thunder";
    public const string DraefendisRageName = "Draefendi's Rage";
    public const string SteorrasProphecyName = "Steorra's Prophecy";
    public const string BalogarsBladeName = "Balogar's Blade";
    public const string WinnehildsBattleCryName = "Winnehild's Battle Cry";
    public const string AelfricsAuspicesName = "Aelfric's Auspices";
    public const string SealticgesSeductionName = "Sealticge's Seduction";

    private static readonly IReadOnlySet<string> DivineSkillNames = new HashSet<string>(StringComparer.Ordinal)
    {
        BrandsThunderName,
        DraefendisRageName,
        SteorrasProphecyName,
        BalogarsBladeName,
        WinnehildsBattleCryName,
        AelfricsAuspicesName,
        SealticgesSeductionName
    };

    public static bool IsDivineSkill(SkillDefinition skill)
        => IsDivineSkill(skill.Name);

    public static bool IsDivineSkill(string skillName)
        => DivineSkillNames.Contains(skillName);
}

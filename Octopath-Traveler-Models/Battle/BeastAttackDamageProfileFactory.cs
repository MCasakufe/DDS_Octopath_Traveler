namespace Octopath_Traveler_Models.Battle;

internal sealed class BeastAttackDamageProfileFactory
{
    private const string HalfCurrentHpSkillName = "Vortal Claw";
    private const int MinimumConfiguredHitCount = 0;

    private static readonly HashSet<string> ElementalDamageSkills = new(StringComparer.Ordinal)
    {
        "Ice blast",
        "Meteor Storm",
        "Freeze",
        "Luminescence",
        "Enshadow",
        "Wind slash",
        "Incinerate",
        "Windshot",
        "Firesand",
        "Thundershot",
        "Lightshot",
        "Iceshot",
        "Shadowshot",
        "Black Gale",
        "Galestorm"
    };

    public BeastAttackDamageProfile Create(BeastCombatUnit beast)
    {
        BeastAttackDamageKind damageKind = SelectDamageKind(beast.GetAssignedSkillName());
        int hitCount = SelectHitCount(beast.GetAssignedSkillHits(), damageKind);
        return new BeastAttackDamageProfile(damageKind, hitCount);
    }

    private static BeastAttackDamageKind SelectDamageKind(string skillName)
    {
        if (skillName == HalfCurrentHpSkillName)
            return BeastAttackDamageKind.HalfCurrentHp;

        return ElementalDamageSkills.Contains(skillName)
            ? BeastAttackDamageKind.Elemental
            : BeastAttackDamageKind.Physical;
    }

    private static int SelectHitCount(int configuredHits, BeastAttackDamageKind damageKind)
    {
        if (damageKind == BeastAttackDamageKind.HalfCurrentHp)
            return 1;

        return configuredHits <= MinimumConfiguredHitCount ? MinimumConfiguredHitCount : configuredHits;
    }
}

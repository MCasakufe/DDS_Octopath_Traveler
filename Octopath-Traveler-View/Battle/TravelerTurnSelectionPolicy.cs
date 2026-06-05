using Octopath_Traveler_Models.Battle;
using Octopath_Traveler_Models.RuntimeData;

namespace Octopath_Traveler_View.Battle;

internal sealed class TravelerTurnSelectionPolicy
{
    private const string SingleTarget = "Single";
    private const string AllyTarget = "Ally";
    private const string NightmareChimeraSkillName = "Nightmare Chimera";

    private static readonly IReadOnlyList<string> NightmareChimeraWeaponTypes =
    [
        "Sword",
        "Spear",
        "Dagger",
        "Axe",
        "Bow",
        "Stave"
    ];

    private static readonly IReadOnlySet<string> ReviveOnlyAllySkills = new HashSet<string>(StringComparer.Ordinal)
    {
        "Vivify"
    };

    private static readonly IReadOnlySet<string> AutoSelectedBeastTargetSkills = new HashSet<string>(StringComparer.Ordinal)
    {
        "Thousand Spears",
        "Rain of Arrows",
        "Guardian Liondog"
    };

    public IReadOnlyList<SkillDefinition> SelectCastableSkills(TravelerCombatUnit traveler)
        => traveler.AssignedActiveSkills
            .Where(skill => IsCastableSkill(traveler, skill))
            .ToList();

    public IReadOnlyList<BeastCombatUnit> SelectBeastTargets(BattleState battleState)
        => battleState.BeastTeam
            .Where(beast => beast.IsAlive)
            .ToList();

    public TravelerSkillInputPlan CreateSkillInputPlan(
        SkillDefinition selectedSkill,
        TravelerCombatUnit traveler,
        BattleState battleState)
        => new(
            SelectWeaponTypes(selectedSkill),
            SelectTargetInputKind(selectedSkill, traveler),
            SelectTravelerTargets(selectedSkill, traveler, battleState));

    public bool RequiresBpSelection(SkillDefinition selectedSkill)
        => !TravelerDivineSkillCatalog.IsDivineSkill(selectedSkill);

    public int SelectAutomaticBpCost(SkillDefinition selectedSkill)
        => TravelerDivineSkillCatalog.IsDivineSkill(selectedSkill)
            ? TravelerDivineSkillCatalog.RequiredBpCost
            : 0;

    private static bool IsCastableSkill(TravelerCombatUnit traveler, SkillDefinition skill)
        => traveler.CurrentSp >= traveler.CalculateSkillSpCost(skill.Sp) && HasRequiredBp(traveler, skill);

    private static bool HasRequiredBp(TravelerCombatUnit traveler, SkillDefinition skill)
        => !TravelerDivineSkillCatalog.IsDivineSkill(skill)
           || traveler.CurrentBp >= TravelerDivineSkillCatalog.RequiredBpCost;

    private static IReadOnlyList<string> SelectWeaponTypes(SkillDefinition selectedSkill)
        => selectedSkill.Name == NightmareChimeraSkillName ? NightmareChimeraWeaponTypes : [];

    private static TravelerSkillTargetInputKind SelectTargetInputKind(
        SkillDefinition selectedSkill,
        TravelerCombatUnit traveler)
    {
        if (traveler.HasTargetModificationStatus && IsTargetModifiedBySealticge(selectedSkill))
            return TravelerSkillTargetInputKind.None;

        if (AutoSelectedBeastTargetSkills.Contains(selectedSkill.Name))
            return TravelerSkillTargetInputKind.None;

        if (selectedSkill.Target == SingleTarget)
            return TravelerSkillTargetInputKind.Beast;

        return selectedSkill.Target == AllyTarget
            ? TravelerSkillTargetInputKind.Traveler
            : TravelerSkillTargetInputKind.None;
    }

    private static IReadOnlyList<TravelerCombatUnit> SelectTravelerTargets(
        SkillDefinition selectedSkill,
        TravelerCombatUnit traveler,
        BattleState battleState)
    {
        if (traveler.HasTargetModificationStatus && IsTargetModifiedBySealticge(selectedSkill))
            return [];

        if (selectedSkill.Target != AllyTarget)
            return [];

        bool requiresDeadAllies = ReviveOnlyAllySkills.Contains(selectedSkill.Name);
        return battleState.TravelerTeam
            .Where(traveler => requiresDeadAllies ? !traveler.IsAlive : traveler.IsAlive)
            .ToList();
    }

    private static bool IsTargetModifiedBySealticge(SkillDefinition selectedSkill)
        => selectedSkill.Target is SingleTarget or AllyTarget;
}

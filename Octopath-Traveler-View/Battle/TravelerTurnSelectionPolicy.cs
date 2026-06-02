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

    public IReadOnlyList<SkillDefinition> SelectCastableSkills(TravelerCombatUnit traveler)
        => traveler.AssignedActiveSkills
            .Where(skill => traveler.CurrentSp >= skill.Sp)
            .ToList();

    public IReadOnlyList<BeastCombatUnit> SelectBeastTargets(BattleState battleState)
        => battleState.BeastTeam
            .Where(beast => beast.IsAlive)
            .ToList();

    public TravelerSkillInputPlan CreateSkillInputPlan(SkillDefinition selectedSkill, BattleState battleState)
        => new(
            SelectWeaponTypes(selectedSkill),
            SelectTargetInputKind(selectedSkill),
            SelectTravelerTargets(selectedSkill, battleState));

    private static IReadOnlyList<string> SelectWeaponTypes(SkillDefinition selectedSkill)
        => selectedSkill.Name == NightmareChimeraSkillName ? NightmareChimeraWeaponTypes : [];

    private static TravelerSkillTargetInputKind SelectTargetInputKind(SkillDefinition selectedSkill)
    {
        if (selectedSkill.Target == SingleTarget)
            return TravelerSkillTargetInputKind.Beast;

        return selectedSkill.Target == AllyTarget
            ? TravelerSkillTargetInputKind.Traveler
            : TravelerSkillTargetInputKind.None;
    }

    private static IReadOnlyList<TravelerCombatUnit> SelectTravelerTargets(
        SkillDefinition selectedSkill,
        BattleState battleState)
    {
        if (selectedSkill.Target != AllyTarget)
            return [];

        bool requiresDeadAllies = ReviveOnlyAllySkills.Contains(selectedSkill.Name);
        return battleState.TravelerTeam
            .Where(traveler => requiresDeadAllies ? !traveler.IsAlive : traveler.IsAlive)
            .ToList();
    }
}

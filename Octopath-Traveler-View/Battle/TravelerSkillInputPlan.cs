using Octopath_Traveler_Models.Battle;

namespace Octopath_Traveler_View.Battle;

internal enum TravelerSkillTargetInputKind
{
    None,
    Beast,
    Traveler
}

internal sealed record TravelerSkillInputPlan(
    IReadOnlyList<string> SelectableWeaponTypes,
    TravelerSkillTargetInputKind TargetInputKind,
    IReadOnlyList<TravelerCombatUnit> SelectableTravelerTargets)
{
    public bool RequiresWeaponSelection => SelectableWeaponTypes.Count > 0;
}

using Octopath_Traveler_Models.Battle;

namespace Octopath_Traveler_View.Battle;

internal sealed record BasicAttackSelection(
    string SelectedWeapon,
    BeastCombatUnit SelectedTarget,
    int UsedBp);

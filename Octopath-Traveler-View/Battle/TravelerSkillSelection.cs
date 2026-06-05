using Octopath_Traveler_Models.Battle;
using Octopath_Traveler_Models.RuntimeData;

namespace Octopath_Traveler_View.Battle;

internal sealed record TravelerSkillSelection(
    SkillDefinition SelectedSkill,
    BeastCombatUnit? SelectedBeastTarget,
    TravelerCombatUnit? SelectedTravelerTarget,
    string? SelectedWeapon,
    int UsedBp);

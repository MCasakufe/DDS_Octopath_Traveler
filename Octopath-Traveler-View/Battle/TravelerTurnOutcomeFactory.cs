using Octopath_Traveler_Models.Battle;

namespace Octopath_Traveler_View.Battle;

internal sealed class TravelerTurnOutcomeFactory
{
    public TravelerTurnOutcome? CreateImmediateOutcome(TravelerActionOption? selectedAction)
        => selectedAction switch
        {
            TravelerActionOption.Defend => TravelerTurnOutcome.Defend(),
            TravelerActionOption.Flee => TravelerTurnOutcome.Flee(),
            _ => null
        };

    public TravelerTurnOutcome CreateBasicAttackOutcome(BasicAttackSelection selection)
        => TravelerTurnOutcome.BasicAttack(
            selection.SelectedWeapon,
            selection.SelectedTarget,
            selection.UsedBp);

    public TravelerTurnOutcome CreateSkillOutcome(TravelerSkillSelection selection)
        => TravelerTurnOutcome.Skill(
            selection.SelectedSkill.Name,
            selection.SelectedBeastTarget,
            selection.SelectedTravelerTarget,
            selection.SelectedWeapon,
            selection.UsedBp);
}

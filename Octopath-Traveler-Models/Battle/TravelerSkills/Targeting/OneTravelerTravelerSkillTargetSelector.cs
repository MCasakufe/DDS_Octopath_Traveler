namespace Octopath_Traveler_Models.Battle;

internal sealed class OneTravelerTravelerSkillTargetSelector : TravelerSkillTargetSelector
{
    public override TravelerSkillTargetSelection SelectTargets(TravelerSkillTargetSelectionContext selectionContext)
    {
        TravelerCombatUnit? target = selectionContext.TurnOutcome.SelectedTravelerTarget;
        return target is null ? TravelerSkillTargetSelection.Empty : TravelerSkillTargetSelection.WithTraveler(target);
    }
}

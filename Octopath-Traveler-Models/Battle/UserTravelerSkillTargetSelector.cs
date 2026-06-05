namespace Octopath_Traveler_Models.Battle;

internal sealed class UserTravelerSkillTargetSelector : TravelerSkillTargetSelector
{
    public override TravelerSkillTargetSelection SelectTargets(TravelerSkillTargetSelectionContext selectionContext)
        => TravelerSkillTargetSelection.WithTraveler(selectionContext.Traveler);
}

namespace Octopath_Traveler_Models.Battle;

internal sealed class NoTravelerSkillTargetSelector : TravelerSkillTargetSelector
{
    public override TravelerSkillTargetSelection SelectTargets(TravelerSkillTargetSelectionContext selectionContext)
        => TravelerSkillTargetSelection.Empty;
}

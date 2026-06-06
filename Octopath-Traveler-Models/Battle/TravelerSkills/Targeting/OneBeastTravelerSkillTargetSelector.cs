namespace Octopath_Traveler_Models.Battle;

internal sealed class OneBeastTravelerSkillTargetSelector : TravelerSkillTargetSelector
{
    public override TravelerSkillTargetSelection SelectTargets(TravelerSkillTargetSelectionContext selectionContext)
    {
        BeastCombatUnit? target = selectionContext.TurnOutcome.SelectedBeastTarget;
        return target is null ? TravelerSkillTargetSelection.Empty : TravelerSkillTargetSelection.WithBeast(target);
    }
}

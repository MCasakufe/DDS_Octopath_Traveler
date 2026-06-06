namespace Octopath_Traveler_Models.Battle;

internal sealed class SkillTargetTypeTravelerSkillTargetSelector : TravelerSkillTargetSelector
{
    private readonly IReadOnlyDictionary<string, TravelerSkillTargetSelector> _targetSelectors;
    private readonly TravelerSkillTargetSelector _defaultTargetSelector;

    public SkillTargetTypeTravelerSkillTargetSelector(
        IReadOnlyDictionary<string, TravelerSkillTargetSelector> targetSelectors,
        TravelerSkillTargetSelector defaultTargetSelector)
    {
        _targetSelectors = targetSelectors;
        _defaultTargetSelector = defaultTargetSelector;
    }

    public override TravelerSkillTargetSelection SelectTargets(TravelerSkillTargetSelectionContext selectionContext)
    {
        TravelerSkillTargetSelector targetSelector = SelectTargetSelector(selectionContext.Skill.Target);
        return targetSelector.SelectTargets(selectionContext);
    }

    private TravelerSkillTargetSelector SelectTargetSelector(string targetType)
        => _targetSelectors.TryGetValue(targetType, out TravelerSkillTargetSelector? targetSelector)
            ? targetSelector
            : _defaultTargetSelector;
}

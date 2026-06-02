namespace Octopath_Traveler_Models.Battle;

internal abstract class TravelerSkillBehavior
{
    private readonly IReadOnlyList<TravelerSkillEffect> _effects;

    protected TravelerSkillBehavior(IReadOnlyList<TravelerSkillEffect> effects)
    {
        _effects = effects;
    }

    public bool CanExecuteSkill(string skillName)
        => UsesSkillName(skillName);

    public IReadOnlyList<TravelerSkillResult> Execute(TravelerSkillExecutionContext executionContext)
    {
        TravelerSkillTargetSelector targetSelector = SelectTargetSelector(executionContext);
        TravelerSkillTargetSelection targetSelection = targetSelector.SelectTargets(
            executionContext.BuildTargetSelectionContext());
        TravelerSkillEffectContext effectContext = new(executionContext, targetSelection);

        foreach (TravelerSkillEffect effect in _effects)
            effect.Apply(effectContext);

        return effectContext.Results;
    }

    protected abstract bool UsesSkillName(string skillName);

    protected abstract TravelerSkillTargetSelector SelectTargetSelector(
        TravelerSkillExecutionContext executionContext);
}

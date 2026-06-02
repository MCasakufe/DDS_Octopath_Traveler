namespace Octopath_Traveler_Models.Battle;

internal sealed class SkillNameSetTravelerSkillBehavior : TravelerSkillBehavior
{
    private readonly IReadOnlySet<string> _skillNames;
    private readonly TravelerSkillTargetSelector _targetSelector;

    public SkillNameSetTravelerSkillBehavior(
        IReadOnlySet<string> skillNames,
        TravelerSkillTargetSelector targetSelector,
        params TravelerSkillEffect[] effects)
        : base(effects)
    {
        _skillNames = skillNames;
        _targetSelector = targetSelector;
    }

    protected override bool UsesSkillName(string skillName)
        => _skillNames.Contains(skillName);

    protected override TravelerSkillTargetSelector SelectTargetSelector(
        TravelerSkillExecutionContext executionContext)
        => _targetSelector;
}

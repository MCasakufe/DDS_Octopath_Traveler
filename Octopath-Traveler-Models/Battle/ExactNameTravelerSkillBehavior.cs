namespace Octopath_Traveler_Models.Battle;

internal sealed class ExactNameTravelerSkillBehavior : TravelerSkillBehavior
{
    private readonly string _skillName;
    private readonly TravelerSkillTargetSelector _targetSelector;

    public ExactNameTravelerSkillBehavior(
        string skillName,
        TravelerSkillTargetSelector targetSelector,
        params TravelerSkillEffect[] effects)
        : base(effects)
    {
        _skillName = skillName;
        _targetSelector = targetSelector;
    }

    protected override bool UsesSkillName(string skillName)
        => skillName == _skillName;

    protected override TravelerSkillTargetSelector SelectTargetSelector(
        TravelerSkillExecutionContext executionContext)
        => _targetSelector;
}

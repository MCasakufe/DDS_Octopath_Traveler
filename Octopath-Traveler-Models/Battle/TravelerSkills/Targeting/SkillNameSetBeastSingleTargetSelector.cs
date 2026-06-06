namespace Octopath_Traveler_Models.Battle;

internal abstract class SkillNameSetBeastSingleTargetSelector : BeastSingleTargetSelector
{
    private readonly IReadOnlySet<string> _skillNames;

    protected SkillNameSetBeastSingleTargetSelector(IReadOnlySet<string> skillNames)
    {
        _skillNames = skillNames;
    }

    protected sealed override bool CanSelectTargetForCore(string skillName)
        => _skillNames.Contains(skillName);
}

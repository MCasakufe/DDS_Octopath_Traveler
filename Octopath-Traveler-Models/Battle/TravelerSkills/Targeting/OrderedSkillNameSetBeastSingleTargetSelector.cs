namespace Octopath_Traveler_Models.Battle;

internal abstract class OrderedSkillNameSetBeastSingleTargetSelector : SkillNameSetBeastSingleTargetSelector
{
    protected OrderedSkillNameSetBeastSingleTargetSelector(IReadOnlySet<string> skillNames)
        : base(skillNames)
    {
    }

    protected sealed override TravelerCombatUnit? SelectTargetCore(IReadOnlyList<TravelerCombatUnit> aliveTravelers)
        => OrderTargets(aliveTravelers).FirstOrDefault();

    protected abstract IOrderedEnumerable<TravelerCombatUnit> OrderTargets(
        IReadOnlyList<TravelerCombatUnit> aliveTravelers);
}

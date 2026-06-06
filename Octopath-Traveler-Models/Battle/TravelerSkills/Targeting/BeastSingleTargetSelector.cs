namespace Octopath_Traveler_Models.Battle;

internal abstract class BeastSingleTargetSelector
{
    protected abstract bool CanSelectTargetForCore(string skillName);
    protected abstract TravelerCombatUnit? SelectTargetCore(IReadOnlyList<TravelerCombatUnit> aliveTravelers);

    public bool CanSelectTargetFor(string skillName)
        => CanSelectTargetForCore(skillName);

    public TravelerCombatUnit? SelectTarget(IReadOnlyList<TravelerCombatUnit> aliveTravelers)
        => SelectTargetCore(aliveTravelers);
}

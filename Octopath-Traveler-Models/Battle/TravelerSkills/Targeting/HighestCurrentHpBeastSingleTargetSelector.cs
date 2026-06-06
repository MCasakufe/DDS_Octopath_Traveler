namespace Octopath_Traveler_Models.Battle;

internal sealed class HighestCurrentHpBeastSingleTargetSelector : BeastSingleTargetSelector
{
    protected override bool CanSelectTargetForCore(string skillName)
        => true;

    protected override TravelerCombatUnit? SelectTargetCore(IReadOnlyList<TravelerCombatUnit> aliveTravelers)
        => aliveTravelers
            .OrderByDescending(traveler => traveler.CurrentHp)
            .ThenBy(traveler => traveler.BoardSlotIndex)
            .FirstOrDefault();
}

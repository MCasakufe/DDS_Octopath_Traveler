namespace Octopath_Traveler_Models.Battle;

internal abstract class OrderedBeastTravelerSkillTargetSelector : TravelerSkillTargetSelector
{
    public override TravelerSkillTargetSelection SelectTargets(TravelerSkillTargetSelectionContext selectionContext)
    {
        BeastCombatUnit? target = SelectTarget(selectionContext);
        return target is null ? TravelerSkillTargetSelection.Empty : TravelerSkillTargetSelection.WithBeast(target);
    }

    protected abstract IOrderedEnumerable<BeastCombatUnit> OrderTargets(IReadOnlyList<BeastCombatUnit> aliveBeasts);

    private BeastCombatUnit? SelectTarget(TravelerSkillTargetSelectionContext selectionContext)
    {
        IReadOnlyList<BeastCombatUnit> aliveBeasts = SelectAliveBeasts(selectionContext);
        return OrderTargets(aliveBeasts).FirstOrDefault();
    }

    private static IReadOnlyList<BeastCombatUnit> SelectAliveBeasts(
        TravelerSkillTargetSelectionContext selectionContext)
        => selectionContext.BattleState.BeastTeam
            .Where(beast => beast.IsAlive)
            .ToList();
}

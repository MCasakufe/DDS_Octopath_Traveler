namespace Octopath_Traveler_Models.Battle;

internal abstract class TravelerSkillBehavior
{
    private const string SingleTargetType = "Single";
    private const string AllyTargetType = "Ally";

    private readonly IReadOnlyList<TravelerSkillEffect> _effects;

    protected TravelerSkillBehavior(IReadOnlyList<TravelerSkillEffect> effects)
    {
        _effects = effects;
    }

    public bool CanHandleSkill(string skillName)
        => UsesSkillName(skillName);

    public IReadOnlyList<TravelerSkillResult> Apply(TravelerSkillExecutionContext executionContext)
    {
        TravelerSkillTargetSelector targetSelector = SelectTargetSelector(executionContext);
        TravelerSkillTargetSelection targetSelection = targetSelector.SelectTargets(
            executionContext.BuildTargetSelectionContext());
        targetSelection = ApplyTargetModificationStatus(executionContext, targetSelection);
        TravelerSkillEffectContext effectContext = new(executionContext, targetSelection);

        foreach (TravelerSkillEffect effect in _effects)
            effect.Apply(effectContext);

        return effectContext.Results;
    }

    protected abstract bool UsesSkillName(string skillName);

    protected abstract TravelerSkillTargetSelector SelectTargetSelector(
        TravelerSkillExecutionContext executionContext);

    private static TravelerSkillTargetSelection ApplyTargetModificationStatus(
        TravelerSkillExecutionContext executionContext,
        TravelerSkillTargetSelection targetSelection)
    {
        if (!executionContext.Traveler.HasTargetModificationStatus)
            return targetSelection;

        if (executionContext.SelectedSkill.Target == SingleTargetType)
            return TravelerSkillTargetSelection.WithBeasts(SelectAliveBeasts(executionContext.BattleState));

        return executionContext.SelectedSkill.Target == AllyTargetType
            ? TravelerSkillTargetSelection.WithTravelers(SelectAliveTravelers(executionContext.BattleState))
            : targetSelection;
    }

    private static IReadOnlyList<BeastCombatUnit> SelectAliveBeasts(BattleState battleState)
        => battleState.BeastTeam
            .Where(beast => beast.IsAlive)
            .OrderBy(beast => beast.BoardSlotIndex)
            .ToList();

    private static IReadOnlyList<TravelerCombatUnit> SelectAliveTravelers(BattleState battleState)
        => battleState.TravelerTeam
            .Where(traveler => traveler.IsAlive)
            .OrderBy(traveler => traveler.BoardSlotIndex)
            .ToList();
}

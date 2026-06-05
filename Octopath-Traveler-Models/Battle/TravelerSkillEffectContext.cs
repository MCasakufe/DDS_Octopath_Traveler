using Octopath_Traveler_Models.RuntimeData;

namespace Octopath_Traveler_Models.Battle;

internal sealed class TravelerSkillEffectContext
{
    private readonly List<TravelerSkillResult> _results = [];

    public TravelerSkillEffectContext(
        TravelerSkillExecutionContext executionContext,
        TravelerSkillTargetSelection targetSelection)
    {
        Traveler = executionContext.Traveler;
        BattleState = executionContext.BattleState;
        Skill = executionContext.SelectedSkill;
        TurnOutcome = executionContext.TurnOutcome;
        TargetSelection = targetSelection;
    }

    public TravelerCombatUnit Traveler { get; }

    public BattleState BattleState { get; }

    public SkillDefinition Skill { get; }

    public TravelerTurnOutcome TurnOutcome { get; }

    public TravelerSkillTargetSelection TargetSelection { get; }

    public IReadOnlyList<TravelerSkillResult> Results => _results;

    public void AddResult(TravelerSkillResult result)
        => _results.Add(result);
}

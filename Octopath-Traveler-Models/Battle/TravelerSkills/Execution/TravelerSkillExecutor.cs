using Octopath_Traveler_Models.RuntimeData;

namespace Octopath_Traveler_Models.Battle;

public sealed class TravelerSkillExecutor
{
    private readonly TravelerSkillBehaviorCatalog _skillBehaviorCatalog;

    public TravelerSkillExecutor()
        : this(new TravelerSkillBehaviorCatalog())
    {
    }

    internal TravelerSkillExecutor(TravelerSkillBehaviorCatalog skillBehaviorCatalog)
    {
        ArgumentNullException.ThrowIfNull(skillBehaviorCatalog);

        _skillBehaviorCatalog = skillBehaviorCatalog;
    }

    public TravelerSkillAction ExecuteSkill(TravelerSkillExecutionRequest skillExecutionRequest)
    {
        foreach (SkillDefinition selectedSkill in SelectRequestedSkills(skillExecutionRequest))
            return ExecuteSelectedSkill(skillExecutionRequest, selectedSkill);

        return CreateEmptySkillAction(skillExecutionRequest);
    }

    private TravelerSkillAction ExecuteSelectedSkill(
        TravelerSkillExecutionRequest skillExecutionRequest,
        SkillDefinition selectedSkill)
    {
        if (!IsCastableSkill(skillExecutionRequest, selectedSkill))
            return CreateEmptySkillAction(skillExecutionRequest);

        ConsumeSkillSp(skillExecutionRequest.Traveler, selectedSkill);

        IReadOnlyList<TravelerSkillResult> results = ApplySelectedSkillBehavior(skillExecutionRequest, selectedSkill);

        return new TravelerSkillAction(
            skillExecutionRequest.Traveler.Name,
            skillExecutionRequest.SkillName,
            results);
    }

    private static IEnumerable<SkillDefinition> SelectRequestedSkills(
        TravelerSkillExecutionRequest skillExecutionRequest)
        => skillExecutionRequest.Traveler.AssignedActiveSkills
            .Where(activeSkill => activeSkill.Name == skillExecutionRequest.SkillName);

    private static bool IsCastableSkill(
        TravelerSkillExecutionRequest skillExecutionRequest,
        SkillDefinition selectedSkill)
        => HasRequiredSp(skillExecutionRequest.Traveler, selectedSkill)
           && HasRequiredBp(skillExecutionRequest, selectedSkill);

    private static TravelerSkillAction CreateEmptySkillAction(TravelerSkillExecutionRequest skillExecutionRequest)
        => new(skillExecutionRequest.Traveler.Name, skillExecutionRequest.SkillName, []);

    private static bool HasRequiredSp(TravelerCombatUnit traveler, SkillDefinition selectedSkill)
        => traveler.CurrentSp >= traveler.CalculateSkillSpCost(selectedSkill.Sp);

    private static bool HasRequiredBp(
        TravelerSkillExecutionRequest skillExecutionRequest,
        SkillDefinition selectedSkill)
        => !TravelerDivineSkillCatalog.IsDivineSkill(selectedSkill)
           || (skillExecutionRequest.Traveler.CurrentBp >= TravelerDivineSkillCatalog.RequiredBpCost
               && skillExecutionRequest.TurnOutcome.UsedBp == TravelerDivineSkillCatalog.RequiredBpCost);

    private static void ConsumeSkillSp(TravelerCombatUnit traveler, SkillDefinition selectedSkill)
        => traveler.ConsumeSkillSp(traveler.CalculateSkillSpCost(selectedSkill.Sp));

    private IReadOnlyList<TravelerSkillResult> ApplySelectedSkillBehavior(
        TravelerSkillExecutionRequest skillExecutionRequest,
        SkillDefinition selectedSkill)
    {
        TravelerSkillExecutionContext executionContext = new(skillExecutionRequest, selectedSkill);
        return _skillBehaviorCatalog.ApplyBehavior(executionContext);
    }
}

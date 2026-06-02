namespace Octopath_Traveler_Models.Battle;

public sealed record BeastAttack(
    string AttackerName,
    string SkillName,
    IReadOnlyList<BeastAttackResult> Results);

public sealed class BeastAttackExecutor
{
    private readonly BeastAttackTargetSelector _targetSelector;
    private readonly BeastAttackDamageProfileFactory _damageProfileFactory;
    private readonly BeastAttackHitExecutor _hitExecutor;

    public BeastAttackExecutor()
        : this(
            new BeastAttackTargetSelector(),
            new BeastAttackDamageProfileFactory(),
            new BeastAttackHitExecutor(
                new BeastAttackDamageCalculator(),
                new TravelerDamageApplier()))
    {
    }

    private BeastAttackExecutor(
        BeastAttackTargetSelector targetSelector,
        BeastAttackDamageProfileFactory damageProfileFactory,
        BeastAttackHitExecutor hitExecutor)
    {
        _targetSelector = targetSelector;
        _damageProfileFactory = damageProfileFactory;
        _hitExecutor = hitExecutor;
    }

    public BeastAttack? ExecuteAttack(BeastCombatUnit beast, BattleState battleState)
    {
        IReadOnlyList<TravelerCombatUnit> targets = SelectTargets(beast, battleState);
        if (targets.Count == 0)
            return null;

        BeastAttackDamageProfile damageProfile = _damageProfileFactory.Create(beast);
        if (damageProfile.HitCount == 0)
            return null;

        IReadOnlyList<BeastAttackResult> results = ExecuteAttackAndCollectResults(beast, targets, damageProfile);
        if (results.Count == 0)
            return null;

        return new BeastAttack(beast.Name, beast.GetAssignedSkillName(), results);
    }

    private IReadOnlyList<TravelerCombatUnit> SelectTargets(BeastCombatUnit beast, BattleState battleState)
        => _targetSelector.SelectTargets(new BeastAttackTargetSelectionRequest(
            beast.GetAssignedSkillName(),
            beast.GetAssignedSkillTargetType(),
            battleState));

    private IReadOnlyList<BeastAttackResult> ExecuteAttackAndCollectResults(
        BeastCombatUnit beast,
        IReadOnlyList<TravelerCombatUnit> targets,
        BeastAttackDamageProfile damageProfile)
    {
        List<BeastAttackResult> results = [];
        foreach (TravelerCombatUnit target in targets)
            results.AddRange(ExecuteTargetHits(beast, target, damageProfile));

        foreach (TravelerCombatUnit target in targets.OrderBy(target => target.BoardSlotIndex))
            results.Add(new BeastAttackHpSummaryResult(target.Name, target.CurrentHp));

        return results;
    }

    private IReadOnlyList<BeastAttackResult> ExecuteTargetHits(
        BeastCombatUnit beast,
        TravelerCombatUnit target,
        BeastAttackDamageProfile damageProfile)
    {
        List<BeastAttackResult> results = [];
        for (int hitIndex = 0; hitIndex < damageProfile.HitCount; hitIndex++)
            results.AddRange(ExecuteSingleHit(beast, target, damageProfile.DamageKind));

        return results;
    }

    private IReadOnlyList<BeastAttackResult> ExecuteSingleHit(
        BeastCombatUnit beast,
        TravelerCombatUnit target,
        BeastAttackDamageKind damageKind)
    {
        BeastAttackHitResult hitResult = _hitExecutor.ExecuteHit(new BeastAttackHitExecutionRequest(
            beast,
            target,
            damageKind));
        return BuildSingleHitResults(target.Name, hitResult, damageKind);
    }

    private static IReadOnlyList<BeastAttackResult> BuildSingleHitResults(
        string targetName,
        BeastAttackHitResult hitResult,
        BeastAttackDamageKind damageKind)
    {
        List<BeastAttackResult> results = [];
        if (hitResult.WasDefended)
            results.Add(new BeastAttackDefendResult(targetName));

        results.Add(new BeastAttackDamageResult(targetName, hitResult.Damage, damageKind));
        return results;
    }
}

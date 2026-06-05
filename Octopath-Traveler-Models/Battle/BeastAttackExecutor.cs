namespace Octopath_Traveler_Models.Battle;

public sealed class BeastAttackExecutor
{
    private const int NoAvailableTargets = 0;
    private const int NoConfiguredHits = 0;
    private const int NoAttackResults = 0;
    private const int StatusDurationRounds = 2;
    private const string AugmentedMagicSkillName = "Augmented Magic";
    private const string ConsumeArmorSkillName = "Consume Armor";
    private const string AcidSpraySkillName = "Acid Spray";
    private const string FlapSkillName = "Flap";
    private const string GatherStrengthSkillName = "Gather Strength";
    private const string VolcanoSkillName = "Volcano";

    private static readonly IReadOnlySet<string> TargetStatusOnlySkills = new HashSet<string>(StringComparer.Ordinal)
    {
        ConsumeArmorSkillName,
        AcidSpraySkillName
    };

    private static readonly IReadOnlyDictionary<string, IReadOnlyList<UnitStatusEffectKind>> TargetStatusEffects =
        new Dictionary<string, IReadOnlyList<UnitStatusEffectKind>>(StringComparer.Ordinal)
        {
            [ConsumeArmorSkillName] = [UnitStatusEffectKind.DecreasedPhysicalDefense],
            [AcidSpraySkillName] =
            [
                UnitStatusEffectKind.DecreasedPhysicalDefense,
                UnitStatusEffectKind.DecreasedElementalDefense
            ],
            [VolcanoSkillName] = [UnitStatusEffectKind.DecreasedElementalDefense]
        };

    private static readonly IReadOnlyDictionary<string, IReadOnlyList<UnitStatusEffectKind>> SelfStatusEffects =
        new Dictionary<string, IReadOnlyList<UnitStatusEffectKind>>(StringComparer.Ordinal)
        {
            [FlapSkillName] = [UnitStatusEffectKind.IncreasedSpeed],
            [GatherStrengthSkillName] = [UnitStatusEffectKind.IncreasedPhysicalAttack]
        };

    private static readonly IReadOnlyList<UnitStatusEffectKind> AugmentedMagicStatusEffects =
    [
        UnitStatusEffectKind.IncreasedElementalAttack,
        UnitStatusEffectKind.IncreasedElementalDefense
    ];

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
        if (beast.GetAssignedSkillName() == AugmentedMagicSkillName)
            return ExecuteAugmentedMagic(beast, battleState);

        IReadOnlyList<TravelerCombatUnit> targets = SelectTargets(beast, battleState);
        if (targets.Count == NoAvailableTargets)
            return null;

        if (IsTargetStatusOnlySkill(beast.GetAssignedSkillName()))
            return ExecuteTargetStatusOnlySkill(beast, targets);

        BeastAttackDamageProfile damageProfile = _damageProfileFactory.Create(beast);
        if (damageProfile.HitCount == NoConfiguredHits)
            return null;

        IReadOnlyList<BeastAttackResult> results = ApplyAttackAndCollectResults(beast, targets, damageProfile);
        if (results.Count == NoAttackResults)
            return null;

        return new BeastAttack(beast.Name, beast.GetAssignedSkillName(), results);
    }

    private IReadOnlyList<TravelerCombatUnit> SelectTargets(BeastCombatUnit beast, BattleState battleState)
        => _targetSelector.SelectTargets(new BeastAttackTargetSelectionRequest(
            beast.GetAssignedSkillName(),
            beast.GetAssignedSkillTargetType(),
            battleState));

    private IReadOnlyList<BeastAttackResult> ApplyAttackAndCollectResults(
        BeastCombatUnit beast,
        IReadOnlyList<TravelerCombatUnit> targets,
        BeastAttackDamageProfile damageProfile)
    {
        List<BeastAttackResult> results = [];
        foreach (TravelerCombatUnit target in targets)
        {
            results.AddRange(ApplyTargetHits(beast, target, damageProfile));
            AddTargetStatusResults(results, beast.GetAssignedSkillName(), target);
        }

        AddSelfStatusResults(results, beast.GetAssignedSkillName(), beast);

        foreach (TravelerCombatUnit target in targets.OrderBy(target => target.BoardSlotIndex))
            results.Add(new BeastAttackHpSummaryResult(target.Name, target.CurrentHp));

        return results;
    }

    private IReadOnlyList<BeastAttackResult> ApplyTargetHits(
        BeastCombatUnit beast,
        TravelerCombatUnit target,
        BeastAttackDamageProfile damageProfile)
    {
        List<BeastAttackResult> results = [];
        for (int hitIndex = 0; hitIndex < damageProfile.HitCount; hitIndex++)
            results.AddRange(ApplySingleHit(
                beast,
                target,
                damageProfile.DamageKind,
                shouldIncludeDefendResult: hitIndex == 0));

        return results;
    }

    private IReadOnlyList<BeastAttackResult> ApplySingleHit(
        BeastCombatUnit beast,
        TravelerCombatUnit target,
        BeastAttackDamageKind damageKind,
        bool shouldIncludeDefendResult)
    {
        BeastAttackHitResult hitResult = _hitExecutor.ExecuteHit(new BeastAttackHitExecutionRequest(
            beast,
            target,
            damageKind));
        return BuildSingleHitResults(target.Name, hitResult, damageKind, shouldIncludeDefendResult);
    }

    private static IReadOnlyList<BeastAttackResult> BuildSingleHitResults(
        string targetName,
        BeastAttackHitResult hitResult,
        BeastAttackDamageKind damageKind,
        bool shouldIncludeDefendResult)
    {
        List<BeastAttackResult> results = [];
        if (shouldIncludeDefendResult && hitResult.WasDefended)
            results.Add(new BeastAttackDefendResult(targetName));

        results.Add(new BeastAttackDamageResult(targetName, hitResult.Damage, damageKind));
        return results;
    }

    private BeastAttack? ExecuteAugmentedMagic(BeastCombatUnit beast, BattleState battleState)
    {
        List<BeastAttackResult> results = [];
        foreach (BeastCombatUnit target in battleState.BeastTeam
                     .Where(target => target.IsAlive)
                     .OrderBy(target => target.BoardSlotIndex))
        {
            AddStatusResults(results, target, AugmentedMagicStatusEffects);
        }

        return results.Count == NoAttackResults
            ? null
            : new BeastAttack(beast.Name, beast.GetAssignedSkillName(), results);
    }

    private BeastAttack? ExecuteTargetStatusOnlySkill(
        BeastCombatUnit beast,
        IReadOnlyList<TravelerCombatUnit> targets)
    {
        List<BeastAttackResult> results = [];
        foreach (TravelerCombatUnit target in targets)
            AddTargetStatusResults(results, beast.GetAssignedSkillName(), target);

        return results.Count == NoAttackResults
            ? null
            : new BeastAttack(beast.Name, beast.GetAssignedSkillName(), results);
    }

    private static bool IsTargetStatusOnlySkill(string skillName)
        => TargetStatusOnlySkills.Contains(skillName);

    private static void AddTargetStatusResults(
        List<BeastAttackResult> results,
        string skillName,
        TravelerCombatUnit target)
    {
        if (!TargetStatusEffects.TryGetValue(skillName, out IReadOnlyList<UnitStatusEffectKind>? statusEffects))
            return;

        AddStatusResults(results, target, statusEffects);
    }

    private static void AddSelfStatusResults(
        List<BeastAttackResult> results,
        string skillName,
        BeastCombatUnit beast)
    {
        if (!SelfStatusEffects.TryGetValue(skillName, out IReadOnlyList<UnitStatusEffectKind>? statusEffects))
            return;

        AddStatusResults(results, beast, statusEffects);
    }

    private static void AddStatusResults(
        List<BeastAttackResult> results,
        Unit target,
        IReadOnlyList<UnitStatusEffectKind> statusEffects)
    {
        foreach (UnitStatusEffectKind statusEffect in statusEffects)
        {
            target.ApplyStatusEffect(statusEffect, StatusDurationRounds);
            results.Add(new BeastAttackStatusEffectResult(target.Name, statusEffect, StatusDurationRounds));
        }
    }
}

using Octopath_Traveler_Models.RuntimeData;

namespace Octopath_Traveler_Models.Battle;

public sealed record TravelerSkillAction(
    string TravelerName,
    string SkillName,
    IReadOnlyList<string> ResultLines);

public sealed record TravelerSkillExecutionRequest(
    TravelerCombatUnit Traveler,
    BattleState BattleState,
    TravelerTurnOutcome TurnOutcome,
    string SkillName);

public sealed class TravelerSkillExecutor
{
    private const string SingleTargetType = "Single";
    private const string EnemiesTargetType = "Enemies";
    private const int LegholdTrapDurationRounds = 2;
    private const int ReviveStartingHp = 1;
    private const int MissingHpToPercentageMultiplier = 100;
    private const double LastStandBaseMultiplier = 1.0;
    private const double LastStandMissingHpMultiplierPerPercent = 0.03;

    private static readonly BeastDamageResolver BeastDamageResolver = new();
    private static readonly string[] ShootingStarsDamageTypes = ["Wind", "Light", "Dark"];

    private static readonly HashSet<string> PartyHealingSkills = new(StringComparer.Ordinal)
    {
        "Heal Wounds",
        "Heal More"
    };

    private static readonly HashSet<string> BasicOffensiveSkills = new(StringComparer.Ordinal)
    {
        "Fireball",
        "Icewind",
        "Lightning Bolt",
        "Holy Light",
        "Luminescence",
        "Tradewinds",
        "Trade Tempest",
        "Level Slash",
        "Cross Strike",
        "Moonlight Waltz",
        "Night Ode",
        "Icicle",
        "Amputation",
        "Wildfire",
        "True Strike",
        "Thunderbird",
        "Tiger Rage",
        "Qilin's Horn",
        "Yatagarasu",
        "Fox Spirit",
        "Phoenix Storm"
    };

    private static readonly IReadOnlyList<TravelerSkillBehavior> SkillBehaviors = BuildSkillBehaviors();

    private sealed record TravelerBeastDamageRequest(
        TravelerCombatUnit Traveler,
        BeastCombatUnit Target,
        string DamageType,
        double Modifier);

    private sealed record DamageProfile(
        string DamageType,
        double Modifier);

    private sealed record DamageResultContext(
        BeastCombatUnit Target,
        string DamageType,
        BeastDamageResolution DamageResolution);

    private enum DamageResolutionMode
    {
        Standard,
        KeepTargetAlive
    }

    public TravelerSkillAction ExecuteSkill(TravelerSkillExecutionRequest skillExecutionRequest)
    {
        TravelerCombatUnit traveler = skillExecutionRequest.Traveler;
        string skillName = skillExecutionRequest.SkillName;

        SkillDefinition? selectedSkill = SelectCastableSkill(traveler, skillName);
        if (selectedSkill is null)
            return new TravelerSkillAction(traveler.Name, skillName, []);

        ConsumeSkillSp(traveler, selectedSkill);

        List<string> resultLines = ExecuteSelectedSkill(skillExecutionRequest, selectedSkill);

        return new TravelerSkillAction(traveler.Name, skillName, resultLines);
    }

    private static SkillDefinition? SelectCastableSkill(TravelerCombatUnit traveler, string skillName)
    {
        SkillDefinition? selectedSkill = traveler.AssignedActiveSkills
            .FirstOrDefault(activeSkill => activeSkill.Name == skillName);
        return IsCastableSkill(traveler, selectedSkill) ? selectedSkill : null;
    }

    private static bool IsCastableSkill(TravelerCombatUnit traveler, SkillDefinition? selectedSkill)
        => selectedSkill is not null && traveler.CurrentSp >= selectedSkill.Sp;

    private static void ConsumeSkillSp(TravelerCombatUnit traveler, SkillDefinition selectedSkill)
        => traveler.CurrentSp -= selectedSkill.Sp;

    private static List<string> ExecuteSelectedSkill(
        TravelerSkillExecutionRequest skillExecutionRequest,
        SkillDefinition selectedSkill)
    {
        TravelerSkillExecutionContext executionContext = new(skillExecutionRequest, selectedSkill);
        TravelerSkillBehavior? skillBehavior = SelectSkillBehavior(executionContext.SkillName);
        return skillBehavior is null ? [] : skillBehavior.Execute(executionContext);
    }

    private static IReadOnlyList<TravelerSkillBehavior> BuildSkillBehaviors()
        =>
        [
            new LegholdTrapTravelerSkillBehavior(),
            new SpearheadTravelerSkillBehavior(),
            new FirstAidTravelerSkillBehavior(),
            new VivifyTravelerSkillBehavior(),
            new ReviveTravelerSkillBehavior(),
            new ShootingStarsTravelerSkillBehavior(),
            new NightmareChimeraTravelerSkillBehavior(),
            new LastStandTravelerSkillBehavior(),
            new MercyStrikeTravelerSkillBehavior(),
            new PartyHealingTravelerSkillBehavior(),
            new StandardOffensiveTravelerSkillBehavior()
        ];

    private static TravelerSkillBehavior? SelectSkillBehavior(string skillName)
        => SkillBehaviors.FirstOrDefault(skillBehavior => skillBehavior.Matches(skillName));

    private static List<string> ExecuteLegholdTrap(TravelerTurnOutcome turnOutcome)
    {
        if (turnOutcome.SelectedBeastTarget is null)
            return [];

        turnOutcome.SelectedBeastTarget.RemainingDecreasedPriorityRounds += LegholdTrapDurationRounds;
        return [$"{turnOutcome.SelectedBeastTarget.Name} tendrá menor prioridad de turno durante {LegholdTrapDurationRounds} rondas"];
    }

    private static List<string> ExecuteSpearhead(
        TravelerCombatUnit traveler,
        TravelerTurnOutcome turnOutcome,
        SkillDefinition skill)
    {
        traveler.HasPendingIncreasedPriority = true;
        if (turnOutcome.SelectedBeastTarget is null)
            return [];

        BeastDamageResolution damageResolution = ApplyDamageToBeast(
            BuildTravelerBeastDamageRequest(
                traveler,
                turnOutcome.SelectedBeastTarget,
                new DamageProfile(skill.Type, skill.Modifier)));
        return BuildSingleTargetDamageLines(turnOutcome.SelectedBeastTarget, skill.Type, damageResolution);
    }

    private static List<string> ExecuteFirstAid(
        TravelerCombatUnit traveler,
        TravelerTurnOutcome turnOutcome,
        SkillDefinition skill)
    {
        if (turnOutcome.SelectedTravelerTarget is null)
            return [];

        TravelerCombatUnit target = turnOutcome.SelectedTravelerTarget;
        int healedValue = CalculateHealing(traveler, skill.Modifier);
        target.CurrentHp = Math.Min(target.MaxHp, target.CurrentHp + healedValue);

        return
        [
            $"{target.Name} recupera {healedValue} de vida",
            $"{target.Name} termina con HP:{target.CurrentHp}"
        ];
    }

    private static List<string> ExecuteVivify(
        TravelerCombatUnit traveler,
        TravelerTurnOutcome turnOutcome,
        SkillDefinition skill)
    {
        if (turnOutcome.SelectedTravelerTarget is null)
            return [];

        TravelerCombatUnit target = turnOutcome.SelectedTravelerTarget;
        List<string> resultLines = [];
        if (!target.IsAlive)
        {
            target.CurrentHp = ReviveStartingHp;
            target.IsWaitingForNextRoundAfterRevive = true;
            resultLines.Add($"{target.Name} revive");
        }

        int healedValue = CalculateHealing(traveler, skill.Modifier);
        target.CurrentHp = Math.Min(target.MaxHp, target.CurrentHp + healedValue);
        resultLines.Add($"{target.Name} recupera {healedValue} de vida");
        resultLines.Add($"{target.Name} termina con HP:{target.CurrentHp}");
        return resultLines;
    }

    private static List<string> ExecuteRevive(TravelerCombatUnit traveler, BattleState battleState)
    {
        List<TravelerCombatUnit> reviveTargets = SelectDefeatedTravelersInPartyOrder(traveler, battleState).ToList();
        if (reviveTargets.Count == 0)
            return [];

        List<string> resultLines = [];
        foreach (TravelerCombatUnit target in reviveTargets)
        {
            target.CurrentHp = ReviveStartingHp;
            target.IsWaitingForNextRoundAfterRevive = true;
            resultLines.Add($"{target.Name} revive");
        }

        AppendCurrentHpLines(resultLines, reviveTargets);
        return resultLines;
    }

    private static List<string> ExecuteShootingStars(
        TravelerCombatUnit traveler,
        BattleState battleState,
        SkillDefinition skill)
    {
        List<BeastCombatUnit> targets = SelectAliveBeastsInBoardOrder(battleState).ToList();
        if (targets.Count == 0)
            return [];

        List<string> resultLines = [];

        foreach (BeastCombatUnit target in targets)
        {
            foreach (string damageType in ShootingStarsDamageTypes)
            {
                BeastDamageResolution damageResolution = ApplyDamageToBeast(
                    BuildTravelerBeastDamageRequest(
                        traveler,
                        target,
                        new DamageProfile(damageType, skill.Modifier)));
                AddDamageResultLines(resultLines, new DamageResultContext(target, damageType, damageResolution));
            }
        }

        AppendCurrentHpLines(resultLines, targets);
        return resultLines;
    }

    private static List<string> ExecuteNightmareChimera(
        TravelerCombatUnit traveler,
        TravelerTurnOutcome turnOutcome,
        SkillDefinition skill)
    {
        if (turnOutcome.SelectedBeastTarget is null || string.IsNullOrEmpty(turnOutcome.SelectedWeapon))
            return [];

        BeastCombatUnit target = turnOutcome.SelectedBeastTarget;
        string weaponType = turnOutcome.SelectedWeapon;
        BeastDamageResolution damageResolution = ApplyDamageToBeast(
            BuildTravelerBeastDamageRequest(
                traveler,
                target,
                new DamageProfile(weaponType, skill.Modifier)));
        return BuildSingleTargetDamageLines(target, weaponType, damageResolution);
    }

    private static List<string> ExecuteLastStand(
        TravelerCombatUnit traveler,
        BattleState battleState,
        SkillDefinition skill)
    {
        int missingHpPercentage = CalculateMissingHpPercentage(traveler);
        double damageBonusMultiplier = LastStandBaseMultiplier + missingHpPercentage * LastStandMissingHpMultiplierPerPercent;
        return ExecuteSingleTypeDamageAgainstAliveBeasts(
            battleState,
            skill.Type,
            target => ApplyLastStandDamageToBeast(
                BuildTravelerBeastDamageRequest(
                    traveler,
                    target,
                    new DamageProfile(skill.Type, skill.Modifier)),
                damageBonusMultiplier));
    }

    private static List<string> ExecuteMercyStrike(
        TravelerCombatUnit traveler,
        TravelerTurnOutcome turnOutcome,
        SkillDefinition skill)
    {
        if (turnOutcome.SelectedBeastTarget is null)
            return [];

        BeastCombatUnit target = turnOutcome.SelectedBeastTarget;
        BeastDamageResolution damageResolution = ApplyDamageToBeast(
            BuildTravelerBeastDamageRequest(
                traveler,
                target,
                new DamageProfile(skill.Type, skill.Modifier)),
            DamageResolutionMode.KeepTargetAlive);
        return BuildSingleTargetDamageLines(target, skill.Type, damageResolution);
    }

    private static List<string> ExecutePartyHealing(
        TravelerCombatUnit traveler,
        BattleState battleState,
        SkillDefinition skill)
    {
        List<TravelerCombatUnit> targets = SelectAliveTravelersInPartyOrder(traveler, battleState).ToList();
        if (targets.Count == 0)
            return [];

        int healedValue = CalculateHealing(traveler, skill.Modifier);
        List<string> resultLines = [];
        foreach (TravelerCombatUnit target in targets)
        {
            target.CurrentHp = Math.Min(target.MaxHp, target.CurrentHp + healedValue);
            resultLines.Add($"{target.Name} recupera {healedValue} de vida");
        }

        AppendCurrentHpLines(resultLines, targets);
        return resultLines;
    }

    private static List<string> ExecuteStandardOffensiveSkill(
        TravelerSkillExecutionRequest skillExecutionRequest,
        SkillDefinition skill)
    {
        TravelerCombatUnit traveler = skillExecutionRequest.Traveler;
        BattleState battleState = skillExecutionRequest.BattleState;
        TravelerTurnOutcome turnOutcome = skillExecutionRequest.TurnOutcome;

        return skill.Target switch
        {
            SingleTargetType => ExecuteSingleTargetOffensiveSkill(traveler, turnOutcome, skill),
            EnemiesTargetType => ExecuteEnemiesTargetOffensiveSkill(traveler, battleState, skill),
            _ => []
        };
    }

    private static List<string> ExecuteSingleTargetOffensiveSkill(
        TravelerCombatUnit traveler,
        TravelerTurnOutcome turnOutcome,
        SkillDefinition skill)
    {
        if (turnOutcome.SelectedBeastTarget is null)
            return [];

        BeastCombatUnit target = turnOutcome.SelectedBeastTarget;
        BeastDamageResolution damageResolution = ApplyDamageToBeast(
            BuildTravelerBeastDamageRequest(
                traveler,
                target,
                new DamageProfile(skill.Type, skill.Modifier)));
        return BuildSingleTargetDamageLines(target, skill.Type, damageResolution);
    }

    private static List<string> ExecuteEnemiesTargetOffensiveSkill(
        TravelerCombatUnit traveler,
        BattleState battleState,
        SkillDefinition skill)
    {
        return ExecuteSingleTypeDamageAgainstAliveBeasts(
            battleState,
            skill.Type,
            target => ApplyDamageToBeast(
                BuildTravelerBeastDamageRequest(
                    traveler,
                    target,
                    new DamageProfile(skill.Type, skill.Modifier))));
    }

    private static List<string> ExecuteSingleTypeDamageAgainstAliveBeasts(
        BattleState battleState,
        string damageType,
        Func<BeastCombatUnit, BeastDamageResolution> applyDamage)
    {
        List<BeastCombatUnit> targets = SelectAliveBeastsInBoardOrder(battleState).ToList();
        if (targets.Count == 0)
            return [];

        List<string> resultLines = [];
        foreach (BeastCombatUnit target in targets)
        {
            BeastDamageResolution damageResolution = applyDamage(target);
            AddDamageResultLines(resultLines, new DamageResultContext(target, damageType, damageResolution));
        }

        AppendCurrentHpLines(resultLines, targets);
        return resultLines;
    }

    private static List<string> BuildSingleTargetDamageLines(
        BeastCombatUnit target,
        string damageType,
        BeastDamageResolution damageResolution)
    {
        List<string> resultLines = [];
        AddDamageResultLines(resultLines, new DamageResultContext(target, damageType, damageResolution));
        resultLines.Add($"{target.Name} termina con HP:{target.CurrentHp}");
        return resultLines;
    }

    private static void AddDamageResultLines(
        ICollection<string> resultLines,
        DamageResultContext damageResultContext)
    {
        resultLines.Add(BuildDamageLine(
            damageResultContext.Target.Name,
            damageResultContext.DamageType,
            damageResultContext.DamageResolution));
        if (damageResultContext.DamageResolution.EnteredBreakingPoint)
            resultLines.Add($"{damageResultContext.Target.Name} entra en Breaking Point");
    }

    private static void AppendCurrentHpLines<TUnit>(ICollection<string> resultLines, IEnumerable<TUnit> targets)
        where TUnit : Unit
    {
        foreach (TUnit target in targets)
            resultLines.Add($"{target.Name} termina con HP:{target.CurrentHp}");
    }

    private static string BuildDamageLine(string targetName, string damageType, BeastDamageResolution damageResolution)
    {
        string weaknessSuffix = damageResolution.IsWeaknessHit ? " con debilidad" : string.Empty;
        return $"{targetName} recibe {damageResolution.Damage} de da\u00f1o de tipo {damageType}{weaknessSuffix}";
    }

    private static BeastDamageResolution ApplyDamageToBeast(
        TravelerBeastDamageRequest travelerBeastDamageRequest,
        DamageResolutionMode damageResolutionMode = DamageResolutionMode.Standard)
    {
        BeastHitRequest hitRequest = BuildBeastHitRequest(travelerBeastDamageRequest);
        return damageResolutionMode == DamageResolutionMode.KeepTargetAlive
            ? BeastDamageResolver.ResolveHitKeepingTargetAlive(hitRequest)
            : BeastDamageResolver.ResolveHit(hitRequest);
    }

    private static BeastDamageResolution ApplyLastStandDamageToBeast(
        TravelerBeastDamageRequest travelerBeastDamageRequest,
        double damageBonusMultiplier)
    {
        BeastHitRequest hitRequest = BuildBeastHitRequest(travelerBeastDamageRequest);
        return BeastDamageResolver.ResolveHitWithBonus(hitRequest, damageBonusMultiplier);
    }

    private static TravelerBeastDamageRequest BuildTravelerBeastDamageRequest(
        TravelerCombatUnit traveler,
        BeastCombatUnit target,
        DamageProfile damageProfile)
        => new(traveler, target, damageProfile.DamageType, damageProfile.Modifier);

    private static BeastHitRequest BuildBeastHitRequest(TravelerBeastDamageRequest travelerBeastDamageRequest)
        => new(
            travelerBeastDamageRequest.Traveler.PhysAtk,
            travelerBeastDamageRequest.Traveler.ElemAtk,
            travelerBeastDamageRequest.Target,
            travelerBeastDamageRequest.DamageType,
            travelerBeastDamageRequest.Modifier);

    private static int CalculateHealing(TravelerCombatUnit traveler, double modifier)
    {
        double rawHealing = Math.Floor(traveler.ElemDef * modifier);
        return Math.Max(0, (int)rawHealing);
    }

    private static int CalculateMissingHpPercentage(TravelerCombatUnit traveler)
    {
        if (traveler.MaxHp <= 0 || traveler.CurrentHp >= traveler.MaxHp)
            return 0;

        int missingHp = traveler.MaxHp - traveler.CurrentHp;
        return (int)Math.Floor(missingHp * MissingHpToPercentageMultiplier / (double)traveler.MaxHp);
    }

    private sealed record TravelerSkillExecutionContext(
        TravelerSkillExecutionRequest SkillExecutionRequest,
        SkillDefinition SelectedSkill)
    {
        public TravelerCombatUnit Traveler => SkillExecutionRequest.Traveler;
        public BattleState BattleState => SkillExecutionRequest.BattleState;
        public TravelerTurnOutcome TurnOutcome => SkillExecutionRequest.TurnOutcome;
        public string SkillName => SkillExecutionRequest.SkillName;
    }

    private abstract class TravelerSkillBehavior
    {
        public abstract bool Matches(string skillName);
        public abstract List<string> Execute(TravelerSkillExecutionContext executionContext);
    }

    private abstract class ExactNameTravelerSkillBehavior : TravelerSkillBehavior
    {
        private readonly string _skillName;

        protected ExactNameTravelerSkillBehavior(string skillName)
        {
            _skillName = skillName;
        }

        public sealed override bool Matches(string skillName)
            => skillName == _skillName;
    }

    private abstract class SkillNameSetTravelerSkillBehavior : TravelerSkillBehavior
    {
        private readonly IReadOnlySet<string> _skillNames;

        protected SkillNameSetTravelerSkillBehavior(IReadOnlySet<string> skillNames)
        {
            _skillNames = skillNames;
        }

        public sealed override bool Matches(string skillName)
            => _skillNames.Contains(skillName);
    }

    private sealed class LegholdTrapTravelerSkillBehavior : ExactNameTravelerSkillBehavior
    {
        public LegholdTrapTravelerSkillBehavior()
            : base("Leghold Trap")
        {
        }

        public override List<string> Execute(TravelerSkillExecutionContext executionContext)
            => ExecuteLegholdTrap(executionContext.TurnOutcome);
    }

    private sealed class SpearheadTravelerSkillBehavior : ExactNameTravelerSkillBehavior
    {
        public SpearheadTravelerSkillBehavior()
            : base("Spearhead")
        {
        }

        public override List<string> Execute(TravelerSkillExecutionContext executionContext)
            => ExecuteSpearhead(executionContext.Traveler, executionContext.TurnOutcome, executionContext.SelectedSkill);
    }

    private sealed class FirstAidTravelerSkillBehavior : ExactNameTravelerSkillBehavior
    {
        public FirstAidTravelerSkillBehavior()
            : base("First Aid")
        {
        }

        public override List<string> Execute(TravelerSkillExecutionContext executionContext)
            => ExecuteFirstAid(executionContext.Traveler, executionContext.TurnOutcome, executionContext.SelectedSkill);
    }

    private sealed class VivifyTravelerSkillBehavior : ExactNameTravelerSkillBehavior
    {
        public VivifyTravelerSkillBehavior()
            : base("Vivify")
        {
        }

        public override List<string> Execute(TravelerSkillExecutionContext executionContext)
            => ExecuteVivify(executionContext.Traveler, executionContext.TurnOutcome, executionContext.SelectedSkill);
    }

    private sealed class ReviveTravelerSkillBehavior : ExactNameTravelerSkillBehavior
    {
        public ReviveTravelerSkillBehavior()
            : base("Revive")
        {
        }

        public override List<string> Execute(TravelerSkillExecutionContext executionContext)
            => ExecuteRevive(executionContext.Traveler, executionContext.BattleState);
    }

    private sealed class ShootingStarsTravelerSkillBehavior : ExactNameTravelerSkillBehavior
    {
        public ShootingStarsTravelerSkillBehavior()
            : base("Shooting Stars")
        {
        }

        public override List<string> Execute(TravelerSkillExecutionContext executionContext)
            => ExecuteShootingStars(executionContext.Traveler, executionContext.BattleState, executionContext.SelectedSkill);
    }

    private sealed class NightmareChimeraTravelerSkillBehavior : ExactNameTravelerSkillBehavior
    {
        public NightmareChimeraTravelerSkillBehavior()
            : base("Nightmare Chimera")
        {
        }

        public override List<string> Execute(TravelerSkillExecutionContext executionContext)
            => ExecuteNightmareChimera(executionContext.Traveler, executionContext.TurnOutcome, executionContext.SelectedSkill);
    }

    private sealed class LastStandTravelerSkillBehavior : ExactNameTravelerSkillBehavior
    {
        public LastStandTravelerSkillBehavior()
            : base("Last Stand")
        {
        }

        public override List<string> Execute(TravelerSkillExecutionContext executionContext)
            => ExecuteLastStand(executionContext.Traveler, executionContext.BattleState, executionContext.SelectedSkill);
    }

    private sealed class MercyStrikeTravelerSkillBehavior : ExactNameTravelerSkillBehavior
    {
        public MercyStrikeTravelerSkillBehavior()
            : base("Mercy Strike")
        {
        }

        public override List<string> Execute(TravelerSkillExecutionContext executionContext)
            => ExecuteMercyStrike(executionContext.Traveler, executionContext.TurnOutcome, executionContext.SelectedSkill);
    }

    private sealed class PartyHealingTravelerSkillBehavior : SkillNameSetTravelerSkillBehavior
    {
        public PartyHealingTravelerSkillBehavior()
            : base(PartyHealingSkills)
        {
        }

        public override List<string> Execute(TravelerSkillExecutionContext executionContext)
            => ExecutePartyHealing(executionContext.Traveler, executionContext.BattleState, executionContext.SelectedSkill);
    }

    private sealed class StandardOffensiveTravelerSkillBehavior : SkillNameSetTravelerSkillBehavior
    {
        public StandardOffensiveTravelerSkillBehavior()
            : base(BasicOffensiveSkills)
        {
        }

        public override List<string> Execute(TravelerSkillExecutionContext executionContext)
            => ExecuteStandardOffensiveSkill(executionContext.SkillExecutionRequest, executionContext.SelectedSkill);
    }

    private static IEnumerable<TravelerCombatUnit> SelectAliveTravelersInPartyOrder(
        TravelerCombatUnit traveler,
        BattleState battleState)
    {
        return OrderTargetsByBoardWithUserLast(
            battleState.TravelerTeam.Where(target => target.IsAlive),
            traveler.BoardSlotIndex);
    }

    private static IEnumerable<TravelerCombatUnit> SelectDefeatedTravelersInPartyOrder(
        TravelerCombatUnit traveler,
        BattleState battleState)
    {
        return OrderTargetsByBoardWithUserLast(
            battleState.TravelerTeam.Where(target => !target.IsAlive),
            traveler.BoardSlotIndex);
    }

    private static IEnumerable<BeastCombatUnit> SelectAliveBeastsInBoardOrder(BattleState battleState)
        => battleState.BeastTeam
            .Where(target => target.IsAlive)
            .OrderBy(target => target.BoardSlotIndex);

    private static IReadOnlyList<TUnit> OrderTargetsByBoardWithUserLast<TUnit>(
        IEnumerable<TUnit> targets,
        int userBoardSlotIndex)
        where TUnit : Unit
    {
        List<TUnit> orderedTargets = targets.OrderBy(target => target.BoardSlotIndex).ToList();
        List<TUnit> nonUserTargets = orderedTargets
            .Where(target => target.BoardSlotIndex != userBoardSlotIndex)
            .ToList();
        List<TUnit> userTargets = orderedTargets
            .Where(target => target.BoardSlotIndex == userBoardSlotIndex)
            .ToList();
        nonUserTargets.AddRange(userTargets);
        return nonUserTargets;
    }

}


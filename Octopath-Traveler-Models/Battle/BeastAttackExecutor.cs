namespace Octopath_Traveler_Models.Battle;

public sealed record BeastAttack(
    string AttackerName,
    string SkillName,
    IReadOnlyList<string> ResultLines);

public sealed class BeastAttackExecutor
{
    private const string EnemiesTargetType = "Enemies";
    private const string HalfCurrentHpSkillName = "Vortal Claw";
    private const int HalfCurrentHpRoundingOffset = 1;
    private const int HalfCurrentHpDivisor = 2;
    private const int MinimumConfiguredHitCount = 0;

    private static readonly HashSet<string> ElementalDamageSkills = new(StringComparer.Ordinal)
    {
        "Ice blast",
        "Meteor Storm",
        "Freeze",
        "Luminescence",
        "Enshadow",
        "Wind slash",
        "Incinerate",
        "Windshot",
        "Firesand",
        "Thundershot",
        "Lightshot",
        "Iceshot",
        "Shadowshot",
        "Black Gale",
        "Galestorm"
    };

    private static readonly HashSet<string> HighestElemAtkTargetSkills = new(StringComparer.Ordinal)
    {
        "Befuddling claw"
    };

    private static readonly HashSet<string> LowestPhysDefTargetSkills = new(StringComparer.Ordinal)
    {
        "Stab",
        "Boar Rush",
        "Vorpal Fang"
    };

    private static readonly HashSet<string> HighestSpeedTargetSkills = new(StringComparer.Ordinal)
    {
        "Meteor Storm",
        "Freeze",
        "Luminescence",
        "Enshadow",
        "Wind slash"
    };

    private static readonly HashSet<string> LowestElemDefTargetSkills = new(StringComparer.Ordinal)
    {
        "Windshot",
        "Firesand",
        "Thundershot",
        "Lightshot",
        "Iceshot",
        "Shadowshot"
    };

    private static readonly IReadOnlyList<BeastSingleTargetSelector> SingleTargetSelectors = BuildSingleTargetSelectors();
    private static readonly BeastSingleTargetSelector DefaultSingleTargetSelector = new HighestCurrentHpBeastSingleTargetSelector();

    public BeastAttack? ExecuteAttack(BeastCombatUnit beast, BattleState battleState)
    {
        IReadOnlyList<TravelerCombatUnit> targets = SelectTargetTravelers(
            beast.GetAssignedSkillName(),
            beast.GetAssignedSkillTargetType(),
            battleState);
        if (targets.Count == 0)
            return null;

        BeastDamageKind damageKind = DetermineDamageKind(beast.GetAssignedSkillName());
        int hitCount = DetermineHitCount(beast.GetAssignedSkillHits(), damageKind);
        if (hitCount == 0)
            return null;

        IReadOnlyList<string> resultLines = BuildResultLines(beast, targets, damageKind, hitCount);
        if (resultLines.Count == 0)
            return null;

        return new BeastAttack(beast.Name, beast.GetAssignedSkillName(), resultLines);
    }

    private static IReadOnlyList<string> BuildResultLines(
        BeastCombatUnit attacker,
        IReadOnlyList<TravelerCombatUnit> targets,
        BeastDamageKind damageKind,
        int hitCount)
    {
        List<string> resultLines = [];
        foreach (TravelerCombatUnit target in targets)
        {
            for (int hitIndex = 0; hitIndex < hitCount; hitIndex++)
            {
                if (ShouldPrintDefendMessage(target, damageKind))
                    resultLines.Add($"{target.Name} se defiende");

                int damage = ApplyDamage(attacker, target, attacker.GetAssignedSkillModifier(), damageKind);
                resultLines.Add(BuildDamageLine(target.Name, damage, damageKind));
            }
        }

        foreach (TravelerCombatUnit target in targets.OrderBy(target => target.BoardSlotIndex))
            resultLines.Add($"{target.Name} termina con HP:{target.CurrentHp}");

        return resultLines;
    }

    private static int ApplyDamage(
        BeastCombatUnit attacker,
        TravelerCombatUnit target,
        double modifier,
        BeastDamageKind damageKind)
    {
        int damage = CalculateDamage(attacker, target, modifier, damageKind);
        target.ReceiveDamage(damage);
        return damage;
    }

    private static int CalculateDamage(
        BeastCombatUnit attacker,
        TravelerCombatUnit target,
        double modifier,
        BeastDamageKind damageKind)
    {
        if (damageKind == BeastDamageKind.HalfCurrentHp)
            return (target.CurrentHp + HalfCurrentHpRoundingOffset) / HalfCurrentHpDivisor;

        int attackerStat = damageKind == BeastDamageKind.Elemental ? attacker.ElemAtk : attacker.PhysAtk;
        int defenderStat = damageKind == BeastDamageKind.Elemental ? target.ElemDef : target.PhysDef;
        int baseDamage = Math.Max(0, (int)Math.Floor(attackerStat * modifier - defenderStat));
        return target.IsDefendingCurrentRound ? baseDamage / 2 : baseDamage;
    }

    private static string BuildDamageLine(string targetName, int damage, BeastDamageKind damageKind)
    {
        return damageKind switch
        {
            BeastDamageKind.Physical => $"{targetName} recibe {damage} de da\u00f1o f\u00edsico",
            BeastDamageKind.Elemental => $"{targetName} recibe {damage} de da\u00f1o elemental",
            _ => $"{targetName} recibe {damage} de da\u00f1o"
        };
    }

    private static bool ShouldPrintDefendMessage(TravelerCombatUnit target, BeastDamageKind damageKind)
        => target.IsDefendingCurrentRound && damageKind != BeastDamageKind.HalfCurrentHp;

    private static IReadOnlyList<TravelerCombatUnit> SelectTargetTravelers(
        string skillName,
        string targetType,
        BattleState battleState)
    {
        List<TravelerCombatUnit> aliveTravelers = SelectAliveTravelers(battleState).ToList();
        if (aliveTravelers.Count == 0)
            return [];

        if (targetType == EnemiesTargetType)
            return aliveTravelers.OrderBy(traveler => traveler.BoardSlotIndex).ToList();

        TravelerCombatUnit? selectedTarget = SelectSingleTarget(skillName, aliveTravelers);
        return selectedTarget is null ? [] : [selectedTarget];
    }

    private static TravelerCombatUnit? SelectSingleTarget(
        string skillName,
        IReadOnlyList<TravelerCombatUnit> aliveTravelers)
    {
        BeastSingleTargetSelector selector = SelectSingleTargetSelector(skillName);
        return selector.SelectTarget(aliveTravelers);
    }

    private static IReadOnlyList<BeastSingleTargetSelector> BuildSingleTargetSelectors()
        =>
        [
            new HighestElemAtkBeastSingleTargetSelector(),
            new LowestPhysDefBeastSingleTargetSelector(),
            new HighestSpeedBeastSingleTargetSelector(),
            new LowestElemDefBeastSingleTargetSelector()
        ];

    private static BeastSingleTargetSelector SelectSingleTargetSelector(string skillName)
        => SingleTargetSelectors.FirstOrDefault(selector => selector.Matches(skillName))
            ?? DefaultSingleTargetSelector;

    private static IOrderedEnumerable<TravelerCombatUnit> OrderByHighestCurrentHp(IEnumerable<TravelerCombatUnit> travelers)
        => travelers
            .OrderByDescending(traveler => traveler.CurrentHp)
            .ThenBy(traveler => traveler.BoardSlotIndex);

    private static IOrderedEnumerable<TravelerCombatUnit> OrderByHighestElemAtk(IEnumerable<TravelerCombatUnit> travelers)
        => travelers
            .OrderByDescending(traveler => traveler.ElemAtk)
            .ThenBy(traveler => traveler.BoardSlotIndex);

    private static IOrderedEnumerable<TravelerCombatUnit> OrderByLowestPhysDef(IEnumerable<TravelerCombatUnit> travelers)
        => travelers
            .OrderBy(traveler => traveler.PhysDef)
            .ThenBy(traveler => traveler.BoardSlotIndex);

    private static IOrderedEnumerable<TravelerCombatUnit> OrderByHighestSpeed(IEnumerable<TravelerCombatUnit> travelers)
        => travelers
            .OrderByDescending(traveler => traveler.Speed)
            .ThenBy(traveler => traveler.BoardSlotIndex);

    private static IOrderedEnumerable<TravelerCombatUnit> OrderByLowestElemDef(IEnumerable<TravelerCombatUnit> travelers)
        => travelers
            .OrderBy(traveler => traveler.ElemDef)
            .ThenBy(traveler => traveler.BoardSlotIndex);

    private static IEnumerable<TravelerCombatUnit> SelectAliveTravelers(BattleState battleState)
        => battleState.TravelerTeam.Where(traveler => traveler.IsAlive);

    private static BeastDamageKind DetermineDamageKind(string skillName)
    {
        if (skillName == HalfCurrentHpSkillName)
            return BeastDamageKind.HalfCurrentHp;

        return ElementalDamageSkills.Contains(skillName)
            ? BeastDamageKind.Elemental
            : BeastDamageKind.Physical;
    }

    private static int DetermineHitCount(int configuredHits, BeastDamageKind damageKind)
    {
        if (damageKind == BeastDamageKind.HalfCurrentHp)
            return 1;

        return configuredHits <= MinimumConfiguredHitCount ? MinimumConfiguredHitCount : configuredHits;
    }

    private enum BeastDamageKind
    {
        Physical,
        Elemental,
        HalfCurrentHp
    }

    private abstract class BeastSingleTargetSelector
    {
        protected abstract bool MatchesCore(string skillName);
        protected abstract TravelerCombatUnit? SelectTargetCore(IReadOnlyList<TravelerCombatUnit> aliveTravelers);

        public bool Matches(string skillName)
            => MatchesCore(skillName);

        public TravelerCombatUnit? SelectTarget(IReadOnlyList<TravelerCombatUnit> aliveTravelers)
            => SelectTargetCore(aliveTravelers);
    }

    private abstract class SkillNameSetBeastSingleTargetSelector : BeastSingleTargetSelector
    {
        private readonly IReadOnlySet<string> _skillNames;

        protected SkillNameSetBeastSingleTargetSelector(IReadOnlySet<string> skillNames)
        {
            _skillNames = skillNames;
        }

        protected sealed override bool MatchesCore(string skillName)
            => _skillNames.Contains(skillName);
    }

    private abstract class OrderedSkillNameSetBeastSingleTargetSelector : SkillNameSetBeastSingleTargetSelector
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

    private sealed class HighestElemAtkBeastSingleTargetSelector : OrderedSkillNameSetBeastSingleTargetSelector
    {
        internal HighestElemAtkBeastSingleTargetSelector()
            : base(HighestElemAtkTargetSkills)
        {
        }

        protected override IOrderedEnumerable<TravelerCombatUnit> OrderTargets(
            IReadOnlyList<TravelerCombatUnit> aliveTravelers)
            => OrderByHighestElemAtk(aliveTravelers);
    }

    private sealed class LowestPhysDefBeastSingleTargetSelector : OrderedSkillNameSetBeastSingleTargetSelector
    {
        internal LowestPhysDefBeastSingleTargetSelector()
            : base(LowestPhysDefTargetSkills)
        {
        }

        protected override IOrderedEnumerable<TravelerCombatUnit> OrderTargets(
            IReadOnlyList<TravelerCombatUnit> aliveTravelers)
            => OrderByLowestPhysDef(aliveTravelers);
    }

    private sealed class HighestSpeedBeastSingleTargetSelector : OrderedSkillNameSetBeastSingleTargetSelector
    {
        internal HighestSpeedBeastSingleTargetSelector()
            : base(HighestSpeedTargetSkills)
        {
        }

        protected override IOrderedEnumerable<TravelerCombatUnit> OrderTargets(
            IReadOnlyList<TravelerCombatUnit> aliveTravelers)
            => OrderByHighestSpeed(aliveTravelers);
    }

    private sealed class LowestElemDefBeastSingleTargetSelector : OrderedSkillNameSetBeastSingleTargetSelector
    {
        internal LowestElemDefBeastSingleTargetSelector()
            : base(LowestElemDefTargetSkills)
        {
        }

        protected override IOrderedEnumerable<TravelerCombatUnit> OrderTargets(
            IReadOnlyList<TravelerCombatUnit> aliveTravelers)
            => OrderByLowestElemDef(aliveTravelers);
    }

    private sealed class HighestCurrentHpBeastSingleTargetSelector : BeastSingleTargetSelector
    {
        protected override bool MatchesCore(string skillName)
            => true;

        protected override TravelerCombatUnit? SelectTargetCore(IReadOnlyList<TravelerCombatUnit> aliveTravelers)
            => OrderByHighestCurrentHp(aliveTravelers).FirstOrDefault();
    }
}

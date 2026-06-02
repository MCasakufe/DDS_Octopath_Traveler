namespace Octopath_Traveler_Models.Battle;

internal sealed record BeastAttackTargetSelectionRequest(
    string SkillName,
    string TargetType,
    BattleState BattleState);

internal sealed class BeastAttackTargetSelector
{
    private const string EnemiesTargetType = "Enemies";

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

    private static readonly IReadOnlyList<BeastSingleTargetSelector> SingleTargetSelectors =
        BuildSingleTargetSelectors();

    private static readonly BeastSingleTargetSelector DefaultSingleTargetSelector =
        new HighestCurrentHpBeastSingleTargetSelector();

    public IReadOnlyList<TravelerCombatUnit> SelectTargets(BeastAttackTargetSelectionRequest selectionRequest)
    {
        List<TravelerCombatUnit> aliveTravelers = SelectAliveTravelers(selectionRequest.BattleState).ToList();
        if (aliveTravelers.Count == 0)
            return [];

        if (selectionRequest.TargetType == EnemiesTargetType)
            return aliveTravelers.OrderBy(traveler => traveler.BoardSlotIndex).ToList();

        TravelerCombatUnit? selectedTarget = SelectSingleTarget(selectionRequest.SkillName, aliveTravelers);
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
        => SingleTargetSelectors.FirstOrDefault(selector => selector.CanSelectTargetFor(skillName))
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

    private abstract class BeastSingleTargetSelector
    {
        protected abstract bool CanSelectTargetForCore(string skillName);
        protected abstract TravelerCombatUnit? SelectTargetCore(IReadOnlyList<TravelerCombatUnit> aliveTravelers);

        public bool CanSelectTargetFor(string skillName)
            => CanSelectTargetForCore(skillName);

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

        protected sealed override bool CanSelectTargetForCore(string skillName)
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
        protected override bool CanSelectTargetForCore(string skillName)
            => true;

        protected override TravelerCombatUnit? SelectTargetCore(IReadOnlyList<TravelerCombatUnit> aliveTravelers)
            => OrderByHighestCurrentHp(aliveTravelers).FirstOrDefault();
    }
}

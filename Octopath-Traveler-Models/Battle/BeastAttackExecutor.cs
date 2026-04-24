namespace Octopath_Traveler_Models.Battle;

public sealed record BeastAttack(
    string AttackerName,
    string SkillName,
    IReadOnlyList<string> ResultLines);

public sealed class BeastAttackExecutor
{
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

    public BeastAttack? ExecuteAttack(BeastCombatUnit beast, BattleState battleState)
    {
        IReadOnlyList<TravelerCombatUnit> targets = SelectTargets(beast.AssignedSkill.Name, beast.AssignedSkill.Target, battleState);
        if (targets.Count == 0)
            return null;

        BeastDamageKind damageKind = ResolveDamageKind(beast.AssignedSkill.Name);
        int hitCount = ResolveHitCount(beast.AssignedSkill.Hits, damageKind);
        if (hitCount == 0)
            return null;

        IReadOnlyList<string> resultLines = BuildResultLines(beast, targets, damageKind, hitCount);
        if (resultLines.Count == 0)
            return null;

        return new BeastAttack(beast.Name, beast.AssignedSkill.Name, resultLines);
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

                int damage = ApplyDamage(attacker, target, attacker.AssignedSkill.Modifier, damageKind);
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
        target.CurrentHp = Math.Max(0, target.CurrentHp - damage);
        return damage;
    }

    private static int CalculateDamage(
        BeastCombatUnit attacker,
        TravelerCombatUnit target,
        double modifier,
        BeastDamageKind damageKind)
    {
        if (damageKind == BeastDamageKind.HalfCurrentHp)
            return (target.CurrentHp + 1) / 2;

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

    private static IReadOnlyList<TravelerCombatUnit> SelectTargets(
        string skillName,
        string targetType,
        BattleState battleState)
    {
        List<TravelerCombatUnit> aliveTravelers = GetAliveTravelers(battleState).ToList();
        if (aliveTravelers.Count == 0)
            return [];

        if (targetType == "Enemies")
            return aliveTravelers.OrderBy(traveler => traveler.BoardSlotIndex).ToList();

        TravelerCombatUnit? selectedTarget = SelectSingleTarget(skillName, aliveTravelers);
        return selectedTarget is null ? [] : [selectedTarget];
    }

    private static TravelerCombatUnit? SelectSingleTarget(
        string skillName,
        IReadOnlyList<TravelerCombatUnit> aliveTravelers)
    {
        if (HighestElemAtkTargetSkills.Contains(skillName))
            return OrderByHighestElemAtk(aliveTravelers).FirstOrDefault();

        if (LowestPhysDefTargetSkills.Contains(skillName))
            return OrderByLowestPhysDef(aliveTravelers).FirstOrDefault();

        if (HighestSpeedTargetSkills.Contains(skillName))
            return OrderByHighestSpeed(aliveTravelers).FirstOrDefault();

        if (LowestElemDefTargetSkills.Contains(skillName))
            return OrderByLowestElemDef(aliveTravelers).FirstOrDefault();

        return OrderByHighestCurrentHp(aliveTravelers).FirstOrDefault();
    }

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

    private static IEnumerable<TravelerCombatUnit> GetAliveTravelers(BattleState battleState)
        => battleState.TravelerTeam.Where(traveler => traveler.IsAlive);

    private static BeastDamageKind ResolveDamageKind(string skillName)
    {
        if (skillName == "Vortal Claw")
            return BeastDamageKind.HalfCurrentHp;

        return ElementalDamageSkills.Contains(skillName)
            ? BeastDamageKind.Elemental
            : BeastDamageKind.Physical;
    }

    private static int ResolveHitCount(int configuredHits, BeastDamageKind damageKind)
    {
        if (damageKind == BeastDamageKind.HalfCurrentHp)
            return 1;

        return configuredHits <= 0 ? 0 : configuredHits;
    }

    private enum BeastDamageKind
    {
        Physical,
        Elemental,
        HalfCurrentHp
    }
}

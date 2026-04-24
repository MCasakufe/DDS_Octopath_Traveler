using Octopath_Traveler_Models.RuntimeData;

namespace Octopath_Traveler_Models.Battle;

public sealed record TravelerSkillAction(
    string TravelerName,
    string SkillName,
    IReadOnlyList<string> ResultLines);

public sealed class TravelerSkillExecutor
{
    private static readonly BeastDamageResolver BeastDamageResolver = new();

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

    public TravelerSkillAction ExecuteSkill(
        TravelerCombatUnit traveler,
        BattleState battleState,
        TravelerTurnOutcome turnOutcome,
        string skillName,
        int usedBp)
    {
        _ = usedBp;

        SkillDefinition? skill = traveler.AssignedActiveSkills.FirstOrDefault(activeSkill => activeSkill.Name == skillName);
        if (skill is null || traveler.CurrentSp < skill.Sp)
            return new TravelerSkillAction(traveler.Name, skillName, []);

        traveler.CurrentSp -= skill.Sp;

        List<string> resultLines = skillName switch
        {
            "Leghold Trap" => ExecuteLegholdTrap(turnOutcome),
            "Spearhead" => ExecuteSpearhead(traveler, turnOutcome, skill),
            "First Aid" => ExecuteFirstAid(traveler, turnOutcome, skill),
            "Vivify" => ExecuteVivify(traveler, turnOutcome, skill),
            "Revive" => ExecuteRevive(traveler, battleState),
            "Shooting Stars" => ExecuteShootingStars(traveler, battleState, skill),
            "Nightmare Chimera" => ExecuteNightmareChimera(traveler, turnOutcome, skill),
            "Last Stand" => ExecuteLastStand(traveler, battleState, skill),
            "Mercy Strike" => ExecuteMercyStrike(traveler, turnOutcome, skill),
            _ when PartyHealingSkills.Contains(skillName) => ExecutePartyHealing(traveler, battleState, skill),
            _ when BasicOffensiveSkills.Contains(skillName) => ExecuteStandardOffensiveSkill(traveler, battleState, turnOutcome, skill),
            _ => []
        };

        return new TravelerSkillAction(traveler.Name, skillName, resultLines);
    }

    private static List<string> ExecuteLegholdTrap(TravelerTurnOutcome turnOutcome)
    {
        if (turnOutcome.SelectedBeastTarget is null)
            return [];

        turnOutcome.SelectedBeastTarget.RemainingDecreasedPriorityRounds += 2;
        return [$"{turnOutcome.SelectedBeastTarget.Name} tendrá menor prioridad de turno durante 2 rondas"];
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
            traveler,
            turnOutcome.SelectedBeastTarget,
            skill.Type,
            skill.Modifier,
            keepTargetAtOneHp: false);
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
            target.CurrentHp = 1;
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
        List<TravelerCombatUnit> reviveTargets = GetDeadTravelersInPartyOrder(traveler, battleState).ToList();
        if (reviveTargets.Count == 0)
            return [];

        List<string> resultLines = [];
        foreach (TravelerCombatUnit target in reviveTargets)
        {
            target.CurrentHp = 1;
            target.IsWaitingForNextRoundAfterRevive = true;
            resultLines.Add($"{target.Name} revive");
        }

        foreach (TravelerCombatUnit target in reviveTargets)
            resultLines.Add($"{target.Name} termina con HP:{target.CurrentHp}");

        return resultLines;
    }

    private static List<string> ExecuteShootingStars(
        TravelerCombatUnit traveler,
        BattleState battleState,
        SkillDefinition skill)
    {
        List<BeastCombatUnit> targets = GetAliveBeastsInBoardOrder(battleState).ToList();
        if (targets.Count == 0)
            return [];

        string[] shootingStarTypes = ["Wind", "Light", "Dark"];
        List<BeastCombatUnit> affectedTargets = [];
        List<string> resultLines = [];

        foreach (BeastCombatUnit target in targets)
        {
            foreach (string damageType in shootingStarTypes)
            {
                BeastDamageResolution damageResolution = ApplyDamageToBeast(
                    traveler,
                    target,
                    damageType,
                    skill.Modifier,
                    keepTargetAtOneHp: false);
                AddDamageResultLines(resultLines, target, damageType, damageResolution);
            }

            affectedTargets.Add(target);
        }

        foreach (BeastCombatUnit target in OrderTargetsByBoard(affectedTargets))
            resultLines.Add($"{target.Name} termina con HP:{target.CurrentHp}");

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
            traveler,
            target,
            weaponType,
            skill.Modifier,
            keepTargetAtOneHp: false);
        return BuildSingleTargetDamageLines(target, weaponType, damageResolution);
    }

    private static List<string> ExecuteLastStand(
        TravelerCombatUnit traveler,
        BattleState battleState,
        SkillDefinition skill)
    {
        List<BeastCombatUnit> targets = GetAliveBeastsInBoardOrder(battleState).ToList();
        if (targets.Count == 0)
            return [];

        int missingHpPercentage = CalculateMissingHpPercentage(traveler);
        double damageBonusMultiplier = 1 + missingHpPercentage * 0.03;

        List<BeastCombatUnit> affectedTargets = [];
        List<string> resultLines = [];
        foreach (BeastCombatUnit target in targets)
        {
            BeastDamageResolution damageResolution = ApplyLastStandDamageToBeast(
                traveler,
                target,
                skill.Type,
                skill.Modifier,
                damageBonusMultiplier);
            AddDamageResultLines(resultLines, target, skill.Type, damageResolution);
            affectedTargets.Add(target);
        }

        foreach (BeastCombatUnit target in OrderTargetsByBoard(affectedTargets))
            resultLines.Add($"{target.Name} termina con HP:{target.CurrentHp}");

        return resultLines;
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
            traveler,
            target,
            skill.Type,
            skill.Modifier,
            keepTargetAtOneHp: true);
        return BuildSingleTargetDamageLines(target, skill.Type, damageResolution);
    }

    private static List<string> ExecutePartyHealing(
        TravelerCombatUnit traveler,
        BattleState battleState,
        SkillDefinition skill)
    {
        List<TravelerCombatUnit> targets = GetAliveTravelersInPartyOrder(traveler, battleState).ToList();
        if (targets.Count == 0)
            return [];

        int healedValue = CalculateHealing(traveler, skill.Modifier);
        List<string> resultLines = [];
        foreach (TravelerCombatUnit target in targets)
        {
            target.CurrentHp = Math.Min(target.MaxHp, target.CurrentHp + healedValue);
            resultLines.Add($"{target.Name} recupera {healedValue} de vida");
        }

        foreach (TravelerCombatUnit target in targets)
            resultLines.Add($"{target.Name} termina con HP:{target.CurrentHp}");

        return resultLines;
    }

    private static List<string> ExecuteStandardOffensiveSkill(
        TravelerCombatUnit traveler,
        BattleState battleState,
        TravelerTurnOutcome turnOutcome,
        SkillDefinition skill)
    {
        return skill.Target switch
        {
            "Single" => ExecuteSingleTargetOffensiveSkill(traveler, turnOutcome, skill),
            "Enemies" => ExecuteEnemiesTargetOffensiveSkill(traveler, battleState, skill),
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
            traveler,
            target,
            skill.Type,
            skill.Modifier,
            keepTargetAtOneHp: false);
        return BuildSingleTargetDamageLines(target, skill.Type, damageResolution);
    }

    private static List<string> ExecuteEnemiesTargetOffensiveSkill(
        TravelerCombatUnit traveler,
        BattleState battleState,
        SkillDefinition skill)
    {
        List<BeastCombatUnit> targets = GetAliveBeastsInBoardOrder(battleState).ToList();
        if (targets.Count == 0)
            return [];

        List<BeastCombatUnit> affectedTargets = [];
        List<string> resultLines = [];
        foreach (BeastCombatUnit target in targets)
        {
            BeastDamageResolution damageResolution = ApplyDamageToBeast(
                traveler,
                target,
                skill.Type,
                skill.Modifier,
                keepTargetAtOneHp: false);
            AddDamageResultLines(resultLines, target, skill.Type, damageResolution);
            affectedTargets.Add(target);
        }

        foreach (BeastCombatUnit target in OrderTargetsByBoard(affectedTargets))
            resultLines.Add($"{target.Name} termina con HP:{target.CurrentHp}");

        return resultLines;
    }

    private static List<string> BuildSingleTargetDamageLines(
        BeastCombatUnit target,
        string damageType,
        BeastDamageResolution damageResolution)
    {
        List<string> resultLines = [];
        AddDamageResultLines(resultLines, target, damageType, damageResolution);
        resultLines.Add($"{target.Name} termina con HP:{target.CurrentHp}");
        return resultLines;
    }

    private static void AddDamageResultLines(
        ICollection<string> resultLines,
        BeastCombatUnit target,
        string damageType,
        BeastDamageResolution damageResolution)
    {
        resultLines.Add(BuildDamageLine(target.Name, damageResolution.Damage, damageType, damageResolution.IsWeaknessHit));
        if (damageResolution.EnteredBreakingPoint)
            resultLines.Add($"{target.Name} entra en Breaking Point");
    }

    private static string BuildDamageLine(string targetName, int damage, string damageType, bool isWeaknessHit)
    {
        string weaknessSuffix = isWeaknessHit ? " con debilidad" : string.Empty;
        return $"{targetName} recibe {damage} de daño de tipo {damageType}{weaknessSuffix}";
    }

    private static BeastDamageResolution ApplyDamageToBeast(
        TravelerCombatUnit traveler,
        BeastCombatUnit target,
        string damageType,
        double modifier,
        bool keepTargetAtOneHp)
    {
        return keepTargetAtOneHp
            ? BeastDamageResolver.ResolveHitKeepingTargetAlive(traveler.PhysAtk, traveler.ElemAtk, target, damageType, modifier)
            : BeastDamageResolver.ResolveHit(traveler.PhysAtk, traveler.ElemAtk, target, damageType, modifier);
    }

    private static BeastDamageResolution ApplyLastStandDamageToBeast(
        TravelerCombatUnit traveler,
        BeastCombatUnit target,
        string damageType,
        double modifier,
        double damageBonusMultiplier)
    {
        return BeastDamageResolver.ResolveHitWithBonus(
            traveler.PhysAtk,
            traveler.ElemAtk,
            target,
            damageType,
            modifier,
            damageBonusMultiplier);
    }

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
        return (int)Math.Floor(missingHp * 100.0 / traveler.MaxHp);
    }

    private static IEnumerable<TravelerCombatUnit> GetAliveTravelersInPartyOrder(
        TravelerCombatUnit traveler,
        BattleState battleState)
    {
        return OrderTargetsByBoardWithUserLast(
            battleState.TravelerTeam.Where(target => target.IsAlive),
            traveler.BoardSlotIndex);
    }

    private static IEnumerable<TravelerCombatUnit> GetDeadTravelersInPartyOrder(
        TravelerCombatUnit traveler,
        BattleState battleState)
    {
        return OrderTargetsByBoardWithUserLast(
            battleState.TravelerTeam.Where(target => !target.IsAlive),
            traveler.BoardSlotIndex);
    }

    private static IEnumerable<BeastCombatUnit> GetAliveBeastsInBoardOrder(BattleState battleState)
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

    private static IReadOnlyList<TUnit> OrderTargetsByBoard<TUnit>(IEnumerable<TUnit> targets)
        where TUnit : Unit
    {
        return targets
            .OrderBy(target => target.BoardSlotIndex)
            .ToList();
    }
}

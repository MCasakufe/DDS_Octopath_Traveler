namespace Octopath_Traveler_Models.Battle;

internal sealed class SteorrasProphecyTravelerSkillEffect : TravelerSkillEffect
{
    private const double TeamBpModifierIncrease = 0.2;

    public override void Apply(TravelerSkillEffectContext effectContext)
    {
        IReadOnlyList<BeastCombatUnit> targets = effectContext.TargetSelection.BeastTargets;
        if (targets.Count == 0)
            return;

        TravelerSkillDamageProfile damageProfile = BuildDamageProfile(effectContext);
        foreach (BeastCombatUnit target in targets)
            ApplyDamage(effectContext, target, damageProfile);

        AddCurrentHpLines(effectContext, targets);
    }

    private static TravelerSkillDamageProfile BuildDamageProfile(TravelerSkillEffectContext effectContext)
    {
        int teamBpAfterCost = CalculateTeamBpAfterDivineCost(effectContext);
        double modifier = effectContext.Skill.Modifier
                          + effectContext.Skill.Modifier * TeamBpModifierIncrease * teamBpAfterCost;
        return new TravelerSkillDamageProfile(effectContext.Skill.Type, modifier);
    }

    private static int CalculateTeamBpAfterDivineCost(TravelerSkillEffectContext effectContext)
    {
        int teamBp = effectContext.BattleState.TravelerTeam.Sum(traveler => traveler.CurrentBp);
        return Math.Max(0, teamBp - TravelerDivineSkillCatalog.RequiredBpCost);
    }

    private static void ApplyDamage(
        TravelerSkillEffectContext effectContext,
        BeastCombatUnit target,
        TravelerSkillDamageProfile damageProfile)
    {
        BeastDamageResolution damageResolution = ResolveStandardBeastDamage(
            effectContext,
            target,
            damageProfile);
        AddBeastDamageResultLines(effectContext, target, damageProfile.DamageType, damageResolution);
    }
}

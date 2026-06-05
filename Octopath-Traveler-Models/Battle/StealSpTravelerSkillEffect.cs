namespace Octopath_Traveler_Models.Battle;

internal sealed class StealSpTravelerSkillEffect : TravelerSkillEffect
{
    private const int SpRecoveryPercentage = 5;
    private const int PercentageDivisor = 100;

    private static readonly TravelerSkillHitCountResolver HitCountResolver = new();

    public override void Apply(TravelerSkillEffectContext effectContext)
    {
        BeastCombatUnit? target = effectContext.TargetSelection.SingleBeastTarget;
        if (target is null)
            return;

        int totalDamage = ApplySkillHits(effectContext, target);
        RecoverTravelerSp(effectContext, totalDamage);
        AddCurrentHpLines(effectContext, [target]);
    }

    private static int ApplySkillHits(TravelerSkillEffectContext effectContext, BeastCombatUnit target)
    {
        TravelerSkillDamageProfile damageProfile = BuildSkillDamageProfile(effectContext);
        int totalDamage = 0;
        int hitCount = HitCountResolver.ResolveHitCount(effectContext.Skill);
        for (int hitIndex = 0; hitIndex < hitCount; hitIndex++)
            totalDamage += ApplyDamage(effectContext, target, damageProfile);

        return totalDamage;
    }

    private static int ApplyDamage(
        TravelerSkillEffectContext effectContext,
        BeastCombatUnit target,
        TravelerSkillDamageProfile damageProfile)
    {
        BeastDamageResolution damageResolution = ResolveStandardBeastDamage(
            effectContext,
            target,
            damageProfile);
        AddBeastDamageResultLines(effectContext, target, damageProfile.DamageType, damageResolution);
        return damageResolution.Damage;
    }

    private static void RecoverTravelerSp(TravelerSkillEffectContext effectContext, int totalDamage)
    {
        int recoveredSp = totalDamage * SpRecoveryPercentage / PercentageDivisor;
        if (recoveredSp <= 0)
            return;

        effectContext.Traveler.RecoverSp(recoveredSp);
        effectContext.AddResult(new TravelerSkillSpRecoveryResult(effectContext.Traveler.Name, recoveredSp));
    }
}

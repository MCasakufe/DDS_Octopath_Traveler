namespace Octopath_Traveler_Models.Battle;

internal sealed class HpThiefTravelerSkillEffect : TravelerSkillEffect
{
    private const int HpRecoveryDivisor = 2;

    private static readonly TravelerSkillHitCountResolver HitCountResolver = new();

    public override void Apply(TravelerSkillEffectContext effectContext)
    {
        BeastCombatUnit? target = effectContext.TargetSelection.SingleBeastTarget;
        if (target is null)
            return;

        for (int activationIndex = 0; activationIndex < effectContext.NonDivineSkillActivationCount; activationIndex++)
        {
            int totalDamage = ApplySkillHits(effectContext, target);
            RecoverTravelerHp(effectContext, totalDamage);
        }

        AddCurrentHpLines(effectContext, new Unit[] { target, effectContext.Traveler });
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

    private static void RecoverTravelerHp(TravelerSkillEffectContext effectContext, int totalDamage)
    {
        int recoveredHp = effectContext.Traveler.CalculateReceivedHpThiefHealing(totalDamage / HpRecoveryDivisor);
        effectContext.Traveler.RecoverHp(recoveredHp);
        effectContext.AddResult(new TravelerSkillHealingResult(effectContext.Traveler.Name, recoveredHp));
    }
}

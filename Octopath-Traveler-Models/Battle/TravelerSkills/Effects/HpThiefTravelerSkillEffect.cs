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
        => ApplyStandardSkillHitsAndSumDamage(effectContext, target, HitCountResolver);

    private static void RecoverTravelerHp(TravelerSkillEffectContext effectContext, int totalDamage)
    {
        int recoveredHp = effectContext.Traveler.CalculateReceivedHpThiefHealing(totalDamage / HpRecoveryDivisor);
        effectContext.Traveler.RecoverHp(recoveredHp);
        effectContext.AddResult(new TravelerSkillHealingResult(effectContext.Traveler.Name, recoveredHp));
    }
}

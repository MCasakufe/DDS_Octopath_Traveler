namespace Octopath_Traveler_Models.Battle;

internal sealed class EtherealHealingTravelerSkillEffect : TravelerSkillEffect
{
    private const int BaseDurationRounds = 2;

    public override void Apply(TravelerSkillEffectContext effectContext)
    {
        int durationRounds = BaseDurationRounds + effectContext.TurnOutcome.UsedBp;
        ApplyHpRestorationStatusToTargets(effectContext, durationRounds);
    }

    private static void ApplyHpRestorationStatusToTargets(
        TravelerSkillEffectContext effectContext,
        int durationRounds)
    {
        foreach (TravelerCombatUnit target in effectContext.TargetSelection.TravelerTargets)
            ApplyHpRestorationStatus(effectContext, target, durationRounds);
    }

    private static void ApplyHpRestorationStatus(
        TravelerSkillEffectContext effectContext,
        TravelerCombatUnit target,
        int durationRounds)
    {
        for (int activationIndex = 0; activationIndex < effectContext.NonDivineSkillActivationCount; activationIndex++)
        {
            int appliedDurationRounds = target.ApplyHpRestorationStatus(durationRounds);
            effectContext.AddResult(new TravelerSkillHpRestorationStatusResult(
                target.Name,
                appliedDurationRounds));
        }
    }
}

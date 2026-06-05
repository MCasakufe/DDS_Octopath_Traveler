namespace Octopath_Traveler_Models.Battle;

internal sealed class SealticgesSeductionTravelerSkillEffect : TravelerSkillEffect
{
    private const int DurationRounds = 3;

    public override void Apply(TravelerSkillEffectContext effectContext)
    {
        foreach (TravelerCombatUnit target in effectContext.TargetSelection.TravelerTargets)
        {
            int appliedDurationRounds = target.ApplyTargetModificationStatus(DurationRounds);
            effectContext.AddResult(new TravelerSkillTargetModificationStatusResult(
                target.Name,
                appliedDurationRounds));
        }
    }
}

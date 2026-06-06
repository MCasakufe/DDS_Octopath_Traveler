namespace Octopath_Traveler_Models.Battle;

internal sealed class AelfricsAuspicesTravelerSkillEffect : TravelerSkillEffect
{
    private const int DurationRounds = 3;

    public override void Apply(TravelerSkillEffectContext effectContext)
    {
        foreach (TravelerCombatUnit target in effectContext.TargetSelection.TravelerTargets)
        {
            int appliedDurationRounds = target.ApplySkillActivationDuplicationStatus(DurationRounds);
            effectContext.AddResult(new TravelerSkillActivationDuplicationStatusResult(
                target.Name,
                appliedDurationRounds));
        }
    }
}

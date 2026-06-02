namespace Octopath_Traveler_Models.Battle;

internal sealed class QueueTravelerPriorityTravelerSkillEffect : TravelerSkillEffect
{
    public override void Apply(TravelerSkillEffectContext effectContext)
        => effectContext.Traveler.QueueIncreasedPriorityForNextRound();
}

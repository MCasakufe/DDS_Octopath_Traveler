namespace Octopath_Traveler_Models.Battle;

internal sealed class DecreaseBeastPriorityTravelerSkillEffect : TravelerSkillEffect
{
    private readonly int _durationRounds;

    public DecreaseBeastPriorityTravelerSkillEffect(int durationRounds)
    {
        _durationRounds = durationRounds;
    }

    public override void Apply(TravelerSkillEffectContext effectContext)
    {
        BeastCombatUnit? target = effectContext.TargetSelection.SingleBeastTarget;
        if (target is null)
            return;

        int boostedDurationRounds = CalculateBoostedDuration(effectContext, _durationRounds);
        target.ApplyDecreasedPriorityRounds(boostedDurationRounds);
        effectContext.AddResult(new TravelerSkillPriorityChangeResult(target.Name, boostedDurationRounds));
    }
}

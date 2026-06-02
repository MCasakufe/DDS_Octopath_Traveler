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

        target.ApplyDecreasedPriorityRounds(_durationRounds);
        effectContext.AddResult(new TravelerSkillPriorityChangeResult(target.Name, _durationRounds));
    }
}

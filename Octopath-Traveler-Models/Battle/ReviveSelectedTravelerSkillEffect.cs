namespace Octopath_Traveler_Models.Battle;

internal sealed class ReviveSelectedTravelerSkillEffect : TravelerSkillEffect
{
    private readonly int _reviveStartingHp;

    public ReviveSelectedTravelerSkillEffect(int reviveStartingHp)
    {
        _reviveStartingHp = reviveStartingHp;
    }

    public override void Apply(TravelerSkillEffectContext effectContext)
    {
        TravelerCombatUnit? target = effectContext.TargetSelection.SingleTravelerTarget;
        if (target is null || target.IsAlive)
            return;

        target.ReviveForNextRound(_reviveStartingHp);
        effectContext.AddResult(new TravelerSkillReviveResult(target.Name));
    }
}

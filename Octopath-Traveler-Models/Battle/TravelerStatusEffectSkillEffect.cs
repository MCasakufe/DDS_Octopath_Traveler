namespace Octopath_Traveler_Models.Battle;

internal sealed class TravelerStatusEffectSkillEffect : TravelerSkillEffect
{
    private readonly IReadOnlyList<UnitStatusEffectKind> _statusEffectKinds;
    private readonly int _baseDurationRounds;

    public TravelerStatusEffectSkillEffect(
        UnitStatusEffectKind statusEffectKind,
        int baseDurationRounds)
        : this([statusEffectKind], baseDurationRounds)
    {
    }

    public TravelerStatusEffectSkillEffect(
        IReadOnlyList<UnitStatusEffectKind> statusEffectKinds,
        int baseDurationRounds)
    {
        _statusEffectKinds = statusEffectKinds;
        _baseDurationRounds = baseDurationRounds;
    }

    public override void Apply(TravelerSkillEffectContext effectContext)
    {
        int boostedDurationRounds = CalculateBoostedDuration(effectContext, _baseDurationRounds);
        foreach (TravelerCombatUnit target in effectContext.TargetSelection.TravelerTargets)
            ApplyStatusEffects(effectContext, target, boostedDurationRounds);

        foreach (BeastCombatUnit target in effectContext.TargetSelection.BeastTargets)
            ApplyStatusEffects(effectContext, target, boostedDurationRounds);
    }

    private void ApplyStatusEffects(
        TravelerSkillEffectContext effectContext,
        Unit target,
        int durationRounds)
    {
        foreach (UnitStatusEffectKind statusEffectKind in _statusEffectKinds)
            ApplyStatusEffectAndAddResult(effectContext, target, statusEffectKind, durationRounds);
    }
}

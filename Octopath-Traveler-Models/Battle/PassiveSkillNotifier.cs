namespace Octopath_Traveler_Models.Battle;

public sealed class PassiveSkillNotifier
{
    public static PassiveSkillNotifier Empty { get; } = new([], []);

    private readonly IReadOnlyList<RoundEndPassiveSkillHandler> _roundEndHandlers;
    private readonly IReadOnlyList<ExtraTurnPassiveSkillHandler> _extraTurnHandlers;

    internal PassiveSkillNotifier(
        IReadOnlyList<RoundEndPassiveSkillHandler> roundEndHandlers,
        IReadOnlyList<ExtraTurnPassiveSkillHandler> extraTurnHandlers)
    {
        _roundEndHandlers = roundEndHandlers;
        _extraTurnHandlers = extraTurnHandlers;
    }

    public void NotifyRoundEnded()
    {
        foreach (RoundEndPassiveSkillHandler handler in _roundEndHandlers)
            handler.Handle(new RoundEndPassiveRecoveryContext(handler.Traveler));
    }

    public IReadOnlyList<TravelerCombatUnit> SelectExtraTurnEligibleTravelers(
        ICollection<int> travelersWithGrantedExtraTurn)
    {
        PassiveExtraTurnEligibilityContext context = new(travelersWithGrantedExtraTurn);
        return _extraTurnHandlers
            .Where(handler => handler.CanGrantExtraTurn(context))
            .Select(handler => handler.Traveler)
            .OrderBy(traveler => traveler.BoardSlotIndex)
            .ToList();
    }
}

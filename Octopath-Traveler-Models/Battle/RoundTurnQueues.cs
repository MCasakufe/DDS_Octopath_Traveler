namespace Octopath_Traveler_Models.Battle;

public sealed record RoundTurnQueues(IReadOnlyList<TurnParticipant> CurrentRound, IReadOnlyList<TurnParticipant> NextRound);

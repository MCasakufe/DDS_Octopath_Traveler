using Octopath_Traveler_Models.Battle;

namespace Octopath_Traveler.Battle;

internal sealed class RoundExecutionState
{
    public HashSet<TurnParticipantKey> ActedParticipants { get; } = [];

    public HashSet<int> TravelersWithGrantedPatienceTurn { get; } = [];

    public Queue<int> PendingPatienceTravelerTurns { get; } = new();
}

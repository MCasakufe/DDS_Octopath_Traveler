namespace Octopath_Traveler_Models.Battle;

public sealed record TurnParticipant(
    string Name,
    int Speed,
    BattleSide Side,
    int BoardSlotIndex,
    bool HasRecoveryPriority,
    bool HasDefendPriority,
    bool HasIncreasedPriority,
    bool HasDecreasedPriority);

using Octopath_Traveler_Models.Battle;

namespace Octopath_Traveler.Battle;

public sealed class TravelerDefendTurnCommand
{
    internal void Execute(TravelerCombatUnit traveler)
        => traveler.EnterDefendState();
}

namespace Octopath_Traveler_Models.Battle;

public sealed class BattleWinnerEvaluator
{
    public BattleWinner EvaluateWinner(BattleState battleState)
    {
        if (!HasAnyAliveBeast(battleState))
            return BattleWinner.TravelerTeam;

        if (!HasAnyAliveTraveler(battleState))
            return BattleWinner.EnemyTeam;

        return BattleWinner.None;
    }

    private static bool HasAnyAliveTraveler(BattleState battleState)
        => battleState.TravelerTeam.Any(traveler => traveler.IsAlive);

    private static bool HasAnyAliveBeast(BattleState battleState)
        => battleState.BeastTeam.Any(beast => beast.IsAlive);
}


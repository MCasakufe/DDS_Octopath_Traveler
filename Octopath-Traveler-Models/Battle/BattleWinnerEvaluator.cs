namespace Octopath_Traveler_Models.Battle;

public enum BattleWinner
{
    None,
    TravelerTeam,
    EnemyTeam
}

public sealed class BattleWinnerEvaluator
{
    public BattleWinner GetWinner(BattleState battleState)
    {
        bool hasAliveTraveler = battleState.TravelerTeam.Any(traveler => traveler.IsAlive);
        bool hasAliveBeast = battleState.BeastTeam.Any(beast => beast.IsAlive);

        if (!hasAliveBeast)
            return BattleWinner.TravelerTeam;

        if (!hasAliveTraveler)
            return BattleWinner.EnemyTeam;

        return BattleWinner.None;
    }
}


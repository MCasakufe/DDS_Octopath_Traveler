namespace Octopath_Traveler_Models.Battle;

public enum BattleWinner
{
    None,
    TravelerTeam,
    EnemyTeam
}

public sealed class BattleWinnerEvaluator
{
    public BattleWinner EvaluateWinner(BattleState battleState)
    {
        bool isAnyTravelerAlive = battleState.TravelerTeam.Any(traveler => traveler.IsAlive);
        bool isAnyBeastAlive = battleState.BeastTeam.Any(beast => beast.IsAlive);

        if (!isAnyBeastAlive)
            return BattleWinner.TravelerTeam;

        if (!isAnyTravelerAlive)
            return BattleWinner.EnemyTeam;

        return BattleWinner.None;
    }
}


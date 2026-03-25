using Octopath_Traveler_View;

namespace Octopath_Traveler.Battle;

public enum BattleWinner
{
    None,
    TravelerTeam,
    EnemyTeam
}

public sealed class BattleVictoryResolver
{
    private const string SeparatorLine = "----------------------------------------";

    private readonly View _view;

    public BattleVictoryResolver(View view)
    {
        _view = view;
    }

    public BattleWinner Evaluate(BattleState battleState)
    {
        var hasAliveTraveler = battleState.TravelerTeam.Any(traveler => traveler.IsAlive);
        var hasAliveBeast = battleState.BeastTeam.Any(beast => beast.IsAlive);

        if (!hasAliveBeast)
            return BattleWinner.TravelerTeam;

        if (!hasAliveTraveler)
            return BattleWinner.EnemyTeam;

        return BattleWinner.None;
    }

    public void WriteWinner(BattleWinner winner)
    {
        if (winner == BattleWinner.None)
            return;

        _view.WriteLine(SeparatorLine);
        _view.WriteLine(winner == BattleWinner.TravelerTeam
            ? "Gana equipo del jugador"
            : "Gana equipo del enemigo");
    }

    public void WriteEnemyWinnerAfterFlee()
    {
        _view.WriteLine(SeparatorLine);
        _view.WriteLine("El equipo de viajeros ha huido!");
        _view.WriteLine(SeparatorLine);
        _view.WriteLine("Gana equipo del enemigo");
    }
}
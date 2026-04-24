using Octopath_Traveler.TeamSelection;
using Octopath_Traveler.Battle;
using Octopath_Traveler.RuntimeData;
using Octopath_Traveler_View;

namespace Octopath_Traveler;

public sealed class Game
{
    private const string InvalidTeamFileMessage = "Archivo de equipos no válido";

    private readonly View _view;
    private readonly TeamFileMenu _teamFileMenu;
    private readonly TeamFileParser _teamFileParser;
    private readonly TeamSetupValidator _teamSetupValidator;
    private readonly TeamSetupBattleStateFactory _battleStateFactory;
    private readonly BattleLoopRunner _battleLoopRunner;

    public Game(View view, string teamsFolder)
    {
        _view = view;
        _teamFileMenu = new TeamFileMenu(view, teamsFolder);
        _teamFileParser = new TeamFileParser();
        _teamSetupValidator = new TeamSetupValidator(new JsonValidationCatalogProvider(teamsFolder));
        _battleStateFactory = new TeamSetupBattleStateFactory(new RuntimeDataCatalogProvider(teamsFolder));
        _battleLoopRunner = CreateBattleLoopRunner(view);
    }

    public void Play()
    {
        var battleState = TryLoadBattleState();
        if (battleState is null)
        {
            WriteInvalidTeamFileMessage();
            return;
        }

        _battleLoopRunner.Run(battleState);
    }

    private BattleState? TryLoadBattleState()
    {
        try
        {
            var selectedTeamFilePath = _teamFileMenu.SelectTeamFilePath();
            if (selectedTeamFilePath is null)
                return null;

            var teamSetup = _teamFileParser.Parse(selectedTeamFilePath);
            if (!_teamSetupValidator.IsValid(teamSetup))
                return null;

            return _battleStateFactory.TryCreate(teamSetup);
        }
        catch (TeamFileParseException)
        {
            return null;
        }
        catch (ValidationCatalogLoadException)
        {
            return null;
        }
        catch (RuntimeDataCatalogLoadException)
        {
            return null;
        }
    }

    private void WriteInvalidTeamFileMessage()
        => _view.WriteLine(InvalidTeamFileMessage);

    private static BattleLoopRunner CreateBattleLoopRunner(View view)
    {
        var damageCalculator = new PhysicalAttackDamageCalculator();
        return new BattleLoopRunner(
            new RoundTurnQueueBuilder(),
            new BattleStatePrinter(view),
            new TravelerTurnFlow(view),
            new TravelerBasicAttackExecutor(damageCalculator),
            new BeastAttackExecutor(damageCalculator),
            new BattleActionPrinter(view),
            new BattleWinnerService(view));
    }
}

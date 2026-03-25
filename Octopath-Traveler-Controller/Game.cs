using Octopath_Traveler.TeamSelection;
using Octopath_Traveler.Battle;
using Octopath_Traveler.RuntimeData;
using Octopath_Traveler_View;

namespace Octopath_Traveler;

public class Game
{
    private const string InvalidTeamFileMessage = "Archivo de equipos no válido";

    private readonly View _view;
    private readonly TeamFileMenu _teamFileMenu;
    private readonly TeamFileParser _teamFileParser;
    private readonly TeamSetupValidator _teamSetupValidator;
    private readonly TeamSetupBattleStateFactory _battleStateFactory;

    public Game(View view, string teamsFolder)
    {
        _view = view;
        _teamFileMenu = new TeamFileMenu(view, teamsFolder);
        _teamFileParser = new TeamFileParser();
        _teamSetupValidator = new TeamSetupValidator(new JsonValidationCatalogProvider(teamsFolder));
        _battleStateFactory = new TeamSetupBattleStateFactory(new RuntimeDataCatalogProvider(teamsFolder));
    }

    public void Play()
    {
        var selectedTeamFilePath = _teamFileMenu.SelectTeamFilePath();
        if (selectedTeamFilePath is null)
        {
            WriteInvalidTeamFileMessage();
            return;
        }

        var teamSetup = _teamFileParser.Parse(selectedTeamFilePath);
        if (teamSetup is null)
        {
            WriteInvalidTeamFileMessage();
            return;
        }

        if (!_teamSetupValidator.IsValid(teamSetup))
        {
            WriteInvalidTeamFileMessage();
            return;
        }

        var battleState = _battleStateFactory.TryCreate(teamSetup);
        if (battleState is null)
            WriteInvalidTeamFileMessage();
    }

    private void WriteInvalidTeamFileMessage()
        => _view.WriteLine(InvalidTeamFileMessage);
}

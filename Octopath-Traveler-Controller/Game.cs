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
    private readonly RoundTurnQueueBuilder _roundTurnQueueBuilder;
    private readonly RoundStateRenderer _roundStateRenderer;
    private readonly TravelerTurnFlow _travelerTurnFlow;

    public Game(View view, string teamsFolder)
    {
        _view = view;
        _teamFileMenu = new TeamFileMenu(view, teamsFolder);
        _teamFileParser = new TeamFileParser();
        _teamSetupValidator = new TeamSetupValidator(new JsonValidationCatalogProvider(teamsFolder));
        _battleStateFactory = new TeamSetupBattleStateFactory(new RuntimeDataCatalogProvider(teamsFolder));
        _roundTurnQueueBuilder = new RoundTurnQueueBuilder();
        _roundStateRenderer = new RoundStateRenderer(view);
        _travelerTurnFlow = new TravelerTurnFlow(view);
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
        {
            WriteInvalidTeamFileMessage();
            return;
        }

        var actedParticipants = new HashSet<TurnParticipantKey>();
        var roundTurnQueues = _roundTurnQueueBuilder.CreateQueues(battleState, actedParticipants);
        _roundStateRenderer.RenderRoundState(battleState, roundTurnQueues);

        var travelerTurnOutcome = TryRunNextTravelerTurn(roundTurnQueues, battleState);
        if (travelerTurnOutcome.Resolution == TravelerTurnResolution.Fled)
            WriteEnemyVictoryAfterFlee();
    }

    private TravelerTurnOutcome TryRunNextTravelerTurn(RoundTurnQueues roundTurnQueues, BattleState battleState)
    {
        if (roundTurnQueues.CurrentRound.Count == 0)
            return TravelerTurnOutcome.NoAction();

        var nextParticipant = roundTurnQueues.CurrentRound[0];
        if (nextParticipant.Side != BattleSide.Traveler)
            return TravelerTurnOutcome.NoAction();

        var traveler = battleState.TravelerTeam[nextParticipant.BoardSlotIndex];
        return _travelerTurnFlow.RunTurn(traveler, battleState);
    }

    private void WriteEnemyVictoryAfterFlee()
    {
        _view.WriteLine("----------------------------------------");
        _view.WriteLine("El equipo de viajeros ha huido!");
        _view.WriteLine("----------------------------------------");
        _view.WriteLine("Gana equipo del enemigo");
    }

    private void WriteInvalidTeamFileMessage()
        => _view.WriteLine(InvalidTeamFileMessage);
}

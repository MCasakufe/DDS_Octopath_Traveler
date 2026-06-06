using Octopath_Traveler.Battle;
using Octopath_Traveler_Models.Battle;
using Octopath_Traveler_Models.RuntimeData;
using Octopath_Traveler_Models.TeamSelection;
using Octopath_Traveler_View;
using Octopath_Traveler_View.Battle;
using Octopath_Traveler_View.Main;
using Octopath_Traveler_View.TeamSelection;

namespace Octopath_Traveler;

public sealed class Game
{
    private readonly MainConsoleView _mainConsoleView;
    private readonly TeamSelectionView _teamSelectionView;
    private readonly TeamFileParser _teamFileParser;
    private readonly TeamSetupValidator _teamSetupValidator;
    private readonly TeamSetupBattleStateFactory _battleStateFactory;
    private readonly BattleLoopRunner _battleLoopRunner;

    public Game(View view, string teamsFolder)
        : this(
            new MainConsoleView(view),
            new TeamSelectionView(view, teamsFolder),
            new TeamFileParser(),
            new TeamSetupValidator(new JsonValidationCatalogProvider(teamsFolder)),
            new TeamSetupBattleStateFactory(new RuntimeDataCatalogProvider(teamsFolder)),
            BuildBattleLoopRunner(view))
    {
    }

    public Game(
        MainConsoleView mainConsoleView,
        TeamSelectionView teamSelectionView,
        TeamFileParser teamFileParser,
        TeamSetupValidator teamSetupValidator,
        TeamSetupBattleStateFactory battleStateFactory,
        BattleLoopRunner battleLoopRunner)
    {
        _mainConsoleView = mainConsoleView;
        _teamSelectionView = teamSelectionView;
        _teamFileParser = teamFileParser;
        _teamSetupValidator = teamSetupValidator;
        _battleStateFactory = battleStateFactory;
        _battleLoopRunner = battleLoopRunner;
    }

    public void Play()
    {
        try
        {
            BattleState battleState = LoadBattleState();
            _battleLoopRunner.Run(battleState);
        }
        catch (Exception exception) when (IsInvalidTeamSetupException(exception))
        {
            WriteInvalidTeamFileMessage();
        }
    }

    private BattleState LoadBattleState()
    {
        string selectedTeamSetupFilePath = SelectTeamSetupFilePath();
        return LoadBattleStateFromSelectedFile(selectedTeamSetupFilePath);
    }

    private string SelectTeamSetupFilePath()
    {
        string? selectedTeamSetupFilePath = _teamSelectionView.SelectTeamFilePath();
        if (selectedTeamSetupFilePath is null)
            throw new InvalidTeamSetupException("No valid team file was selected.");

        return selectedTeamSetupFilePath;
    }

    private BattleState LoadBattleStateFromSelectedFile(string selectedTeamSetupFilePath)
    {
        TeamSetup teamSetup = _teamFileParser.Parse(selectedTeamSetupFilePath);
        EnsureTeamSetupIsValid(teamSetup);
        return _battleStateFactory.Create(teamSetup);
    }

    private void EnsureTeamSetupIsValid(TeamSetup teamSetup)
    {
        if (!_teamSetupValidator.IsValid(teamSetup))
            throw new InvalidTeamSetupException("Selected team setup is invalid.");
    }

    private static bool IsInvalidTeamSetupException(Exception exception)
        => exception is InvalidTeamSetupException
           or TeamFileParseException
           or ValidationCatalogLoadException
           or RuntimeDataCatalogLoadException
           or BattleStateCreationException;

    private void WriteInvalidTeamFileMessage()
        => _mainConsoleView.WriteInvalidTeamFileMessage();

    private static BattleLoopRunner BuildBattleLoopRunner(View view)
    {
        PhysicalAttackDamageCalculator damageCalculator = new();
        PhysicalAttackExecutionService physicalAttackExecutionService = new(damageCalculator);
        BattleConsoleView battleConsoleView = new(view);
        TravelerBasicAttackExecutor travelerBasicAttackExecutor = new(physicalAttackExecutionService);
        TravelerSkillExecutor travelerSkillExecutor = new();
        BeastAttackExecutor beastAttackExecutor = new();
        return new BattleLoopRunner(
            new RoundTurnQueueBuilder(),
            battleConsoleView,
            new TravelerBasicAttackTurnCommand(travelerBasicAttackExecutor, battleConsoleView),
            new TravelerSkillTurnCommand(travelerSkillExecutor, battleConsoleView),
            new TravelerDefendTurnCommand(),
            new TravelerFleeTurnCommand(battleConsoleView),
            new BeastActionTurnCommand(beastAttackExecutor, battleConsoleView),
            new BattleWinnerEvaluator());
    }
}

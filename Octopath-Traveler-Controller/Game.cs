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
    {
        _mainConsoleView = new MainConsoleView(view);
        _teamSelectionView = new TeamSelectionView(view, teamsFolder);
        _teamFileParser = new TeamFileParser();
        _teamSetupValidator = new TeamSetupValidator(new JsonValidationCatalogProvider(teamsFolder));
        _battleStateFactory = new TeamSetupBattleStateFactory(new RuntimeDataCatalogProvider(teamsFolder));
        _battleLoopRunner = CreateBattleLoopRunner(view);
    }

    public void Play()
    {
        BattleState? battleState = TryLoadBattleState();
        if (battleState is null)
        {
            _mainConsoleView.WriteInvalidTeamFileMessage();
            return;
        }

        _battleLoopRunner.Run(battleState);
    }

    private BattleState? TryLoadBattleState()
    {
        try
        {
            string? selectedTeamFilePath = _teamSelectionView.SelectTeamFilePath();
            if (selectedTeamFilePath is null)
                return null;

            TeamSetup teamSetup = _teamFileParser.Parse(selectedTeamFilePath);
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

    private static BattleLoopRunner CreateBattleLoopRunner(View view)
    {
        PhysicalAttackDamageCalculator damageCalculator = new();
        PhysicalAttackExecutionService physicalAttackExecutionService = new(damageCalculator);
        return new BattleLoopRunner(
            new RoundTurnQueueBuilder(),
            new BattleConsoleView(view),
            new TravelerBasicAttackExecutor(physicalAttackExecutionService),
            new TravelerSkillExecutor(),
            new BeastAttackExecutor(),
            new BattleWinnerEvaluator());
    }
}

using Octopath_Traveler;
using Octopath_Traveler.Battle;
using Octopath_Traveler_Models.Battle;
using Octopath_Traveler_Models.RuntimeData;
using Octopath_Traveler_Models.TeamSelection;
using Octopath_Traveler_View;
using Octopath_Traveler_View.Battle;
using Octopath_Traveler_View.Main;
using Octopath_Traveler_View.TeamSelection;

const string TestDataFolder = "data";
const string TestGroupPattern = "*-Tests";
const string TestGroupSuffix = "-Tests";
const string TeamsFolderSuffix = "";
const string TestFilePattern = "*.txt";
const string SeparatorLine = "----------------------------------------";

string selectedTestGroupPath = SelectTestGroupPath();
string selectedTestFilePath = SelectTestFilePath(selectedTestGroupPath);
string teamsFolder = selectedTestGroupPath.Replace(TestGroupSuffix, TeamsFolderSuffix);
AnnounceTestCase(selectedTestFilePath);

View view = View.BuildManualTestingView(selectedTestFilePath);
Game game = BuildGame(view, teamsFolder);
game.Play();

Game BuildGame(View view, string teamsFolder)
{
    BattleConsoleView battleConsoleView = new(view);
    return new Game(
        new MainConsoleView(view),
        new TeamSelectionView(view, teamsFolder),
        new TeamFileParser(),
        new TeamSetupValidator(new JsonValidationCatalogProvider(teamsFolder)),
        new TeamSetupBattleStateFactory(new RuntimeDataCatalogProvider(teamsFolder)),
        BuildBattleLoopRunner(battleConsoleView));
}

BattleLoopRunner BuildBattleLoopRunner(BattleConsoleView battleConsoleView)
{
    PhysicalAttackExecutionService physicalAttackExecutionService = BuildPhysicalAttackExecutionService();
    return new BattleLoopRunner(
        new RoundTurnQueueBuilder(),
        battleConsoleView,
        new TravelerBasicAttackTurnCommand(
            new TravelerBasicAttackExecutor(physicalAttackExecutionService),
            battleConsoleView),
        new TravelerSkillTurnCommand(new TravelerSkillExecutor(), battleConsoleView),
        new TravelerDefendTurnCommand(),
        new TravelerFleeTurnCommand(battleConsoleView),
        new BeastActionTurnCommand(new BeastAttackExecutor(), battleConsoleView),
        new BattleWinnerEvaluator());
}

PhysicalAttackExecutionService BuildPhysicalAttackExecutionService()
    => new(new PhysicalAttackDamageCalculator());

string SelectTestGroupPath()
{
    Console.WriteLine("\u00BFQu\u00E9 grupo de test quieres usar?");
    string[] availableTestGroupPaths = GetAvailableTestGroupPathsInOrder();
    WriteOptionList(availableTestGroupPaths);
    return ReadSelectedOption(availableTestGroupPaths);
}

string[] GetAvailableTestGroupPathsInOrder()
{
    string[] availableTestGroupPaths = Directory.GetDirectories(
        TestDataFolder,
        TestGroupPattern,
        SearchOption.TopDirectoryOnly);
    Array.Sort(availableTestGroupPaths);
    return availableTestGroupPaths;
}

void WriteOptionList(string[] options)
{
    for (int optionIndex = 0; optionIndex < options.Length; optionIndex++)
        Console.WriteLine($"{optionIndex}- {options[optionIndex]}");
}

string ReadSelectedOption(string[] options)
{
    int minValue = 0;
    int maxValue = options.Length - 1;
    int selectedOption = ReadSelectedNumber(minValue, maxValue);
    return options[selectedOption];
}

int ReadSelectedNumber(int minValue, int maxValue)
{
    Console.WriteLine($"(Ingresa un n\u00FAmero entre {minValue} y {maxValue})");
    int selectedValue;
    bool wasParsePossible;
    do
    {
        string? userInput = Console.ReadLine();
        wasParsePossible = int.TryParse(userInput, out selectedValue);
    } while (!wasParsePossible || IsOutsideValidRange(minValue, selectedValue, maxValue));

    return selectedValue;
}

bool IsOutsideValidRange(int minValue, int value, int maxValue)
    => value < minValue || value > maxValue;

string SelectTestFilePath(string testGroupPath)
{
    Console.WriteLine("\u00BFQu\u00E9 test quieres ejecutar?");
    string[] availableTestFilePaths = Directory.GetFiles(testGroupPath, TestFilePattern);
    Array.Sort(availableTestFilePaths);
    return ReadSelectedOption(availableTestFilePaths);
}

void AnnounceTestCase(string test)
{
    Console.WriteLine(SeparatorLine);
    Console.WriteLine($"Replicando test: {test}");
    Console.WriteLine($"{SeparatorLine}\n");
}

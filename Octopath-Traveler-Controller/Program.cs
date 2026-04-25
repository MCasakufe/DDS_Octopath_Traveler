using Octopath_Traveler;
using Octopath_Traveler_View;

/* 
 * Este código permite replicar un test case. Primero pregunta por el grupo de test
 * case a replicar. Luego pregunta por el test case específico que se quiere replicar.
 * 
 * Por ejemplo, si tu programa está fallando el test case:
 *      "data/E1-BasicCombat-Tests/006.txt"
 * ... puedes ver qué está ocurriendo mediante correr este programa y decir que quieres
 * replicar del grupo "E1-BasicCombat-Tests" el test case 6.
 * 
 * Al presionar enter, se ingresa el input del test case en forma automática. Si el
 * color es azúl significa que el output de tu programa es el esperado. Si es rojo
 * significa que el output de tu programa es distinto al esperado (i.e., el test falló).
 *
 * Si, por algún motivo, quieres ejecutar tu programa de modo manual (sin replicar un
 * test case específico), puedes cambiar la línea:
 *      var view = View.BuildManualTestingView(test);
 * por:
 *      var view = View.BuildConsoleView();
 */



string selectedTestGroupPath = SelectTestGroupPath();
string selectedTestFilePath = SelectTestFilePath(selectedTestGroupPath);
string teamsFolder = selectedTestGroupPath.Replace("-Tests", "");
AnnounceTestCase(selectedTestFilePath);

View view = View.BuildManualTestingView(selectedTestFilePath);
Game game = new Game(view, teamsFolder);
game.Play();

string SelectTestGroupPath()
{
    Console.WriteLine("¿Qué grupo de test quieres usar?");
    string[] availableTestGroupPaths = GetAvailableTestGroupPathsInOrder();
    WriteOptionList(availableTestGroupPaths);
    return ReadSelectedOption(availableTestGroupPaths);
}

string[] GetAvailableTestGroupPathsInOrder()
{
    string[] availableTestGroupPaths = Directory.GetDirectories("data", "*-Tests", SearchOption.TopDirectoryOnly);
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
    Console.WriteLine($"(Ingresa un número entre {minValue} y {maxValue})");
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
    Console.WriteLine("¿Qué test quieres ejecutar?");
    string[] availableTestFilePaths = Directory.GetFiles(testGroupPath, "*.txt");
    Array.Sort(availableTestFilePaths);
    return ReadSelectedOption(availableTestFilePaths);
}

void AnnounceTestCase(string test)
{
    Console.WriteLine($"----------------------------------------");
    Console.WriteLine($"Replicando test: {test}");
    Console.WriteLine($"----------------------------------------\n");
}

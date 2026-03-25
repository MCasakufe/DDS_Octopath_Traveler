using Octopath_Traveler_View;

namespace Octopath_Traveler.TeamSelection;

public sealed class TeamFileMenu
{
    private readonly View _view;
    private readonly string _teamFilesFolder;

    public TeamFileMenu(View view, string teamFilesFolder)
    {
        _view = view;
        _teamFilesFolder = teamFilesFolder;
    }

    public string? SelectTeamFilePath()
    {
        var availableTeamFileNames = GetAvailableTeamFileNames();
        WriteTeamFileSelection(availableTeamFileNames);

        var selectedIndex = ReadSelectedFileIndex();
        return IsValidFileIndex(selectedIndex, availableTeamFileNames.Count)
            ? BuildTeamFilePath(availableTeamFileNames[selectedIndex!.Value])
            : null;
    }

    private List<string> GetAvailableTeamFileNames()
        => Directory.GetFiles(_teamFilesFolder, "*.txt", SearchOption.TopDirectoryOnly)
            .Select(Path.GetFileName)
            .Where(fileName => !string.IsNullOrWhiteSpace(fileName))
            .Select(fileName => fileName!)
            .OrderBy(fileName => fileName, StringComparer.Ordinal)
            .ToList();

    private void WriteTeamFileSelection(IReadOnlyList<string> teamFileNames)
    {
        _view.WriteLine("Elige un archivo para cargar los equipos");

        for (var index = 0; index < teamFileNames.Count; index++)
            _view.WriteLine($"{index}: {teamFileNames[index]}");
    }

    private int? ReadSelectedFileIndex()
    {
        var selectedIndexText = _view.ReadLine();
        return int.TryParse(selectedIndexText, out var selectedIndex) ? selectedIndex : null;
    }

    private static bool IsValidFileIndex(int? index, int fileCount)
        => index is >= 0 && index < fileCount;

    private string BuildTeamFilePath(string fileName)
        => Path.Combine(_teamFilesFolder, fileName);
}

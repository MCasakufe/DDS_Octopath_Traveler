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

        if (!TryReadSelectedFileIndex(availableTeamFileNames.Count, out var selectedFileIndex))
            return null;

        return BuildTeamFilePath(availableTeamFileNames[selectedFileIndex]);
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

    private bool TryReadSelectedFileIndex(int fileCount, out int selectedFileIndex)
    {
        var selectedIndexText = _view.ReadLine();
        if (!int.TryParse(selectedIndexText, out selectedFileIndex))
            return false;

        return selectedFileIndex >= 0 && selectedFileIndex < fileCount;
    }

    private string BuildTeamFilePath(string fileName)
        => Path.Combine(_teamFilesFolder, fileName);
}

namespace Octopath_Traveler_View.TeamSelection;

public sealed class TeamSelectionView
{
    private readonly View _view;
    private readonly string _teamFilesFolder;

    public TeamSelectionView(View view, string teamFilesFolder)
    {
        _view = view;
        _teamFilesFolder = teamFilesFolder;
    }

    public string? SelectTeamFilePath()
    {
        var availableTeamFileNames = GetAvailableTeamFileNames();
        WriteTeamFileSelection(availableTeamFileNames);

        var selectedFileIndex = ReadSelectedFileIndex(availableTeamFileNames.Count);
        if (selectedFileIndex is null)
            return null;

        return BuildTeamFilePath(availableTeamFileNames[selectedFileIndex.Value]);
    }

    private List<string> GetAvailableTeamFileNames()
    {
        var teamFiles = GetTeamFiles();
        var validFileNames = ExtractValidFileNames(teamFiles);
        return SortFileNames(validFileNames);
    }

    private string[] GetTeamFiles()
        => Directory.GetFiles(_teamFilesFolder, "*.txt", SearchOption.TopDirectoryOnly);

    private IEnumerable<string> ExtractValidFileNames(IEnumerable<string> teamFiles)
        => teamFiles
            .Select(Path.GetFileName)
            .Where(fileName => !string.IsNullOrWhiteSpace(fileName))
            .Select(fileName => fileName!);

    private List<string> SortFileNames(IEnumerable<string> fileNames)
        => fileNames
            .OrderBy(fileName => fileName, StringComparer.Ordinal)
            .ToList();

    private void WriteTeamFileSelection(IReadOnlyList<string> teamFileNames)
    {
        _view.WriteLine("Elige un archivo para cargar los equipos");

        for (var index = 0; index < teamFileNames.Count; index++)
            _view.WriteLine($"{index}: {teamFileNames[index]}");
    }

    private int? ReadSelectedFileIndex(int fileCount)
    {
        var selectedIndexText = _view.ReadLine();
        if (!int.TryParse(selectedIndexText, out var selectedFileIndex))
            return null;

        return selectedFileIndex >= 0 && selectedFileIndex < fileCount
            ? selectedFileIndex
            : null;
    }

    private string BuildTeamFilePath(string fileName)
        => Path.Combine(_teamFilesFolder, fileName);
}

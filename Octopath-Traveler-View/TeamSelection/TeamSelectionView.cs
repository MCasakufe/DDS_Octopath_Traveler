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
        List<string> availableTeamFileNames = GetAvailableTeamFileNames();
        WriteTeamFileSelectionMenu(availableTeamFileNames);

        int? selectedFileIndex = TryReadSelectedFileIndex(availableTeamFileNames.Count);
        if (selectedFileIndex is null)
            return null;

        return BuildSelectedTeamFilePath(availableTeamFileNames[selectedFileIndex.Value]);
    }

    private List<string> GetAvailableTeamFileNames()
    {
        string[] teamFilePaths = GetTeamFilePaths();
        IEnumerable<string> validFileNames = ExtractNonEmptyFileNames(teamFilePaths);
        return SortFileNames(validFileNames);
    }

    private string[] GetTeamFilePaths()
        => Directory.GetFiles(_teamFilesFolder, "*.txt", SearchOption.TopDirectoryOnly);

    private IEnumerable<string> ExtractNonEmptyFileNames(IEnumerable<string> teamFilePaths)
        => teamFilePaths
            .Select(Path.GetFileName)
            .Where(fileName => !string.IsNullOrWhiteSpace(fileName))
            .Select(fileName => fileName!);

    private List<string> SortFileNames(IEnumerable<string> fileNames)
        => fileNames
            .OrderBy(fileName => fileName, StringComparer.Ordinal)
            .ToList();

    private void WriteTeamFileSelectionMenu(IReadOnlyList<string> teamFileNames)
    {
        _view.WriteLine("Elige un archivo para cargar los equipos");

        for (int index = 0; index < teamFileNames.Count; index++)
            _view.WriteLine($"{index}: {teamFileNames[index]}");
    }

    private int? TryReadSelectedFileIndex(int fileCount)
    {
        string? selectedIndexText = _view.ReadLine();
        if (!int.TryParse(selectedIndexText, out int selectedFileIndex))
            return null;

        return selectedFileIndex >= 0 && selectedFileIndex < fileCount
            ? selectedFileIndex
            : null;
    }

    private string BuildSelectedTeamFilePath(string fileName)
        => Path.Combine(_teamFilesFolder, fileName);
}

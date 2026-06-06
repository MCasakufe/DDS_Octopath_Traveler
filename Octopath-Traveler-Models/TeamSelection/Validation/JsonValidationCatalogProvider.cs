using Octopath_Traveler_Models.RuntimeData;

namespace Octopath_Traveler_Models.TeamSelection;

public sealed class JsonValidationCatalogProvider
{
    private readonly ValidationCatalogFileReader _fileReader;
    private readonly JsonValidationCatalogParser _jsonParser = new();

    public JsonValidationCatalogProvider(string teamsDirectoryPath)
    {
        string validationCatalogDirectoryPath = Path.GetDirectoryName(teamsDirectoryPath) ?? string.Empty;
        _fileReader = new ValidationCatalogFileReader(validationCatalogDirectoryPath);
    }

    public ValidationCatalog Load()
    {
        return new ValidationCatalog(
            LoadEntityNamesFromFile(RuntimeDataFileNames.Characters),
            LoadEntityNamesFromFile(RuntimeDataFileNames.Enemies),
            LoadEntityNamesFromFile(RuntimeDataFileNames.ActiveSkills),
            LoadEntityNamesFromFile(RuntimeDataFileNames.PassiveSkills));
    }

    private IReadOnlySet<string> LoadEntityNamesFromFile(string fileName)
    {
        string jsonContent = _fileReader.Read(fileName);
        return _jsonParser.ParseEntityNames(jsonContent, fileName);
    }
}

namespace Octopath_Traveler_Models.TeamSelection;

internal sealed class ValidationCatalogFileReader
{
    private readonly string _validationCatalogDirectoryPath;

    public ValidationCatalogFileReader(string validationCatalogDirectoryPath)
    {
        _validationCatalogDirectoryPath = validationCatalogDirectoryPath;
    }

    public string Read(string fileName)
    {
        string fullPath = Path.Combine(_validationCatalogDirectoryPath, fileName);
        if (!File.Exists(fullPath))
            throw new ValidationCatalogLoadException($"Validation catalog file '{fileName}' was not found.");

        try
        {
            return File.ReadAllText(fullPath);
        }
        catch (IOException exception)
        {
            throw new ValidationCatalogLoadException(
                $"Validation catalog file '{fileName}' could not be read.",
                exception);
        }
        catch (UnauthorizedAccessException exception)
        {
            throw new ValidationCatalogLoadException(
                $"Access to validation catalog file '{fileName}' was denied.",
                exception);
        }
    }
}

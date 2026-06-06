namespace Octopath_Traveler_Models.RuntimeData;

internal sealed class RuntimeDataFileReader
{
    private readonly string _runtimeDataDirectoryPath;

    public RuntimeDataFileReader(string runtimeDataDirectoryPath)
    {
        _runtimeDataDirectoryPath = runtimeDataDirectoryPath;
    }

    public string Read(string fileName)
    {
        string runtimeDataFilePath = Path.Combine(_runtimeDataDirectoryPath, fileName);
        if (!File.Exists(runtimeDataFilePath))
            throw new RuntimeDataCatalogLoadException($"Runtime data file '{fileName}' was not found.");

        try
        {
            return File.ReadAllText(runtimeDataFilePath);
        }
        catch (IOException exception)
        {
            throw new RuntimeDataCatalogLoadException(
                $"Runtime data file '{fileName}' could not be read.",
                exception);
        }
        catch (UnauthorizedAccessException exception)
        {
            throw new RuntimeDataCatalogLoadException(
                $"Access to runtime data file '{fileName}' was denied.",
                exception);
        }
    }
}

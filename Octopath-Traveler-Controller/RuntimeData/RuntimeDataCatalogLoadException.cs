namespace Octopath_Traveler.RuntimeData;

public sealed class RuntimeDataCatalogLoadException : Exception
{
    public RuntimeDataCatalogLoadException(string message)
        : base(message)
    {
    }

    public RuntimeDataCatalogLoadException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

namespace Octopath_Traveler.TeamSelection;

public sealed class ValidationCatalogLoadException : Exception
{
    public ValidationCatalogLoadException(string message)
        : base(message)
    {
    }

    public ValidationCatalogLoadException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

namespace Octopath_Traveler.TeamSelection;

public sealed class TeamFileParseException : Exception
{
    public TeamFileParseException(string message)
        : base(message)
    {
    }

    public TeamFileParseException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

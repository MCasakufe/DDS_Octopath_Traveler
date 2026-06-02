namespace Octopath_Traveler;

internal sealed class InvalidTeamSetupException : Exception
{
    public InvalidTeamSetupException(string message)
        : base(message)
    {
    }
}

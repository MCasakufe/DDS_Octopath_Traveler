namespace Octopath_Traveler_Models.TeamSelection;

internal sealed class TeamFileReader
{
    public string[] ReadLines(string teamFilePath)
    {
        if (!File.Exists(teamFilePath))
            throw new TeamFileParseException("The selected team file does not exist.");

        try
        {
            return File.ReadAllLines(teamFilePath);
        }
        catch (IOException exception)
        {
            throw new TeamFileParseException("Could not read the selected team file.", exception);
        }
        catch (UnauthorizedAccessException exception)
        {
            throw new TeamFileParseException("Access to the selected team file was denied.", exception);
        }
    }
}

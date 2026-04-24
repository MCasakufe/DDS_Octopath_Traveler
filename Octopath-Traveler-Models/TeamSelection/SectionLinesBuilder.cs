namespace Octopath_Traveler_Models.TeamSelection;

internal sealed class SectionLinesBuilder
{
    private readonly List<string> _travelerLines = [];
    private readonly List<string> _beastLines = [];

    public void AddTeamMemberLine(string line, TeamFileSection currentSection)
    {
        if (currentSection == TeamFileSection.PlayerTeam)
        {
            _travelerLines.Add(line);
            return;
        }

        if (currentSection == TeamFileSection.EnemyTeam)
        {
            _beastLines.Add(line);
            return;
        }

        throw new TeamFileParseException("Team member entries must be inside a valid section.");
    }

    public SectionLines Build()
        => new(_travelerLines, _beastLines);
}


namespace Octopath_Traveler.TeamSelection;

internal sealed class SectionLinesBuilder
{
    private readonly List<string> _travelerLines = [];
    private readonly List<string> _beastLines = [];

    public bool TryAddTeamMemberLine(string line, TeamFileSection currentSection)
    {
        if (currentSection == TeamFileSection.PlayerTeam)
        {
            _travelerLines.Add(line);
            return true;
        }

        if (currentSection == TeamFileSection.EnemyTeam)
        {
            _beastLines.Add(line);
            return true;
        }

        return false;
    }

    public SectionLines Build()
        => new(_travelerLines, _beastLines);
}

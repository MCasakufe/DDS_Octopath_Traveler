using System.Text.RegularExpressions;

namespace Octopath_Traveler.TeamSelection;

public sealed class TeamFileParser
{
    public TeamSetup? Parse(string teamFilePath)
    {
        if (!File.Exists(teamFilePath))
            return null;

        try
        {
            var lines = File.ReadAllLines(teamFilePath);
            return ParseTeamSetup(lines);
        }
        catch (IOException)
        {
            return null;
        }
        catch (UnauthorizedAccessException)
        {
            return null;
        }
    }

    private TeamSetup? ParseTeamSetup(IReadOnlyList<string> lines)
    {
        var sectionLines = ParseSectionLines(lines);
        if (sectionLines is null)
            return null;

        var travelers = ParseTravelers(sectionLines.TravelerLines);
        if (travelers is null)
            return null;

        return new TeamSetup(travelers, sectionLines.BeastLines);
    }

    private SectionLines? ParseSectionLines(IReadOnlyList<string> lines)
    {
        var sectionLinesBuilder = new SectionLinesBuilder();
        var currentSection = TeamFileSection.None;

        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();
            if (line.Length == 0)
                continue;

            var parsedSectionHeader = ParseSectionHeader(line);
            if (parsedSectionHeader is not null)
            {
                currentSection = parsedSectionHeader.Value;
                continue;
            }

            if (!sectionLinesBuilder.TryAddTeamMemberLine(line, currentSection))
                return null;
        }

        return sectionLinesBuilder.Build();
    }

    private static TeamFileSection? ParseSectionHeader(string line)
    {
        if (line == "Player Team")
            return TeamFileSection.PlayerTeam;

        if (line == "Enemy Team")
            return TeamFileSection.EnemyTeam;

        return null;
    }

    private static List<TravelerSetup>? ParseTravelers(IEnumerable<string> travelerLines)
    {
        var travelers = new List<TravelerSetup>();
        foreach (var travelerLine in travelerLines)
        {
            var traveler = ParseTravelerLine(travelerLine);
            if (traveler is null)
                return null;

            travelers.Add(traveler);
        }

        return travelers;
    }

    private static TravelerSetup? ParseTravelerLine(string line)
    {
        var travelerName = ExtractTravelerName(line);
        if (string.IsNullOrWhiteSpace(travelerName))
            return null;

        var activeSkillNames = ParseSkillNames(line, '(', ')');
        var passiveSkillNames = ParseSkillNames(line, '[', ']');
        return new TravelerSetup(travelerName, activeSkillNames, passiveSkillNames);
    }

    private static string ExtractTravelerName(string line)
    {
        var firstActiveSkillsIndex = line.IndexOf('(');
        var firstPassiveSkillsIndex = line.IndexOf('[');
        var metadataStartIndex = GetMetadataStartIndex(firstActiveSkillsIndex, firstPassiveSkillsIndex);
        return metadataStartIndex < 0 ? line.Trim() : line[..metadataStartIndex].Trim();
    }

    private static int GetMetadataStartIndex(int firstActiveSkillsIndex, int firstPassiveSkillsIndex)
    {
        if (firstActiveSkillsIndex < 0)
            return firstPassiveSkillsIndex;

        if (firstPassiveSkillsIndex < 0)
            return firstActiveSkillsIndex;

        return Math.Min(firstActiveSkillsIndex, firstPassiveSkillsIndex);
    }

    private static List<string> ParseSkillNames(string line, char startDelimiter, char endDelimiter)
    {
        var escapedStartDelimiter = Regex.Escape(startDelimiter.ToString());
        var escapedEndDelimiter = Regex.Escape(endDelimiter.ToString());
        var segmentPattern = $"{escapedStartDelimiter}([^{escapedEndDelimiter}]*){escapedEndDelimiter}";
        var segmentMatch = Regex.Match(line, segmentPattern);

        if (!segmentMatch.Success)
            return [];

        return segmentMatch.Groups[1].Value
            .Split(',')
            .Select(skillName => skillName.Trim())
            .Where(skillName => skillName.Length > 0)
            .ToList();
    }

    private enum TeamFileSection
    {
        None,
        PlayerTeam,
        EnemyTeam
    }

    private sealed class SectionLinesBuilder
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

    private sealed record SectionLines(IReadOnlyList<string> TravelerLines, IReadOnlyList<string> BeastLines);
}

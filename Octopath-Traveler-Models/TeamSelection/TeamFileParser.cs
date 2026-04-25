using System.Text.RegularExpressions;

namespace Octopath_Traveler_Models.TeamSelection;

public sealed class TeamFileParser
{
    private const string PlayerTeamHeader = "Player Team";
    private const string EnemyTeamHeader = "Enemy Team";

    public TeamSetup Parse(string teamFilePath)
    {
        if (!File.Exists(teamFilePath))
            throw new TeamFileParseException("The selected team file does not exist.");

        try
        {
            string[] lines = File.ReadAllLines(teamFilePath);
            return ParseTeamSetup(lines);
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

    private TeamSetup ParseTeamSetup(IReadOnlyList<string> lines)
    {
        SectionLines sectionLines = ParseSectionLines(lines);
        List<TravelerSetup> travelerSetups = ParseTravelerSetups(sectionLines.TravelerLines);

        return new TeamSetup(travelerSetups, sectionLines.BeastLines);
    }

    private SectionLines ParseSectionLines(IReadOnlyList<string> lines)
    {
        SectionLinesBuilder sectionLinesBuilder = new();
        TeamFileSection currentSection = TeamFileSection.None;

        foreach (string line in ReadNonEmptyLines(lines))
            currentSection = ProcessTeamFileLine(line, sectionLinesBuilder, currentSection);

        return sectionLinesBuilder.Build();
    }

    private static IEnumerable<string> ReadNonEmptyLines(IReadOnlyList<string> lines)
    {
        foreach (string rawLine in lines)
        {
            string line = rawLine.Trim();
            if (line.Length > 0)
                yield return line;
        }
    }

    private static TeamFileSection ProcessTeamFileLine(
        string line,
        SectionLinesBuilder sectionLinesBuilder,
        TeamFileSection currentSection)
    {
        TeamFileSection? parsedSectionHeader = TryParseSectionHeader(line);
        if (parsedSectionHeader is not null)
            return parsedSectionHeader.Value;

        sectionLinesBuilder.AddTeamMemberLine(line, currentSection);
        return currentSection;
    }

    private static TeamFileSection? TryParseSectionHeader(string line)
    {
        if (line == PlayerTeamHeader)
            return TeamFileSection.PlayerTeam;

        if (line == EnemyTeamHeader)
            return TeamFileSection.EnemyTeam;

        return null;
    }

    private static List<TravelerSetup> ParseTravelerSetups(IEnumerable<string> travelerLines)
    {
        List<TravelerSetup> travelerSetups = [];
        foreach (string travelerLine in travelerLines)
            travelerSetups.Add(ParseTravelerSetup(travelerLine));

        return travelerSetups;
    }

    private static TravelerSetup ParseTravelerSetup(string line)
    {
        string travelerName = ExtractTravelerName(line);
        if (string.IsNullOrWhiteSpace(travelerName))
            throw new TeamFileParseException("Traveler names cannot be empty.");

        List<string> activeSkillNames = ParseSkillNames(line, '(', ')');
        List<string> passiveSkillNames = ParseSkillNames(line, '[', ']');
        return new TravelerSetup(travelerName, activeSkillNames, passiveSkillNames);
    }

    private static string ExtractTravelerName(string line)
    {
        int firstActiveSkillsIndex = line.IndexOf('(');
        int firstPassiveSkillsIndex = line.IndexOf('[');
        int metadataStartIndex = GetMetadataStartIndex(firstActiveSkillsIndex, firstPassiveSkillsIndex);
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
        string? segment = TryReadDelimitedSegment(line, startDelimiter, endDelimiter);
        if (segment is null)
            return [];

        return ParseSkillNameList(segment);
    }

    private static string? TryReadDelimitedSegment(string line, char startDelimiter, char endDelimiter)
    {
        string escapedStartDelimiter = Regex.Escape(startDelimiter.ToString());
        string escapedEndDelimiter = Regex.Escape(endDelimiter.ToString());
        string segmentPattern = $"{escapedStartDelimiter}([^{escapedEndDelimiter}]*){escapedEndDelimiter}";
        Match segmentMatch = Regex.Match(line, segmentPattern);

        if (!segmentMatch.Success)
            return null;

        return segmentMatch.Groups[1].Value;
    }

    private static List<string> ParseSkillNameList(string segment)
        => segment
            .Split(',')
            .Select(skillName => skillName.Trim())
            .Where(skillName => skillName.Length > 0)
            .ToList();

}



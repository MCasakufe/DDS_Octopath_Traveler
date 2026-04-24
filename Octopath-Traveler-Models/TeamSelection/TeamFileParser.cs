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
            var lines = File.ReadAllLines(teamFilePath);
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
        var sectionLines = ParseSectionLines(lines);
        var travelers = ParseTravelers(sectionLines.TravelerLines);

        return new TeamSetup(travelers, sectionLines.BeastLines);
    }

    private SectionLines ParseSectionLines(IReadOnlyList<string> lines)
    {
        var sectionLinesBuilder = new SectionLinesBuilder();
        var currentSection = TeamFileSection.None;

        foreach (var line in ReadNonEmptyLines(lines))
            ProcessTeamFileLine(line, sectionLinesBuilder, ref currentSection);

        return sectionLinesBuilder.Build();
    }

    private static IEnumerable<string> ReadNonEmptyLines(IReadOnlyList<string> lines)
    {
        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();
            if (line.Length > 0)
                yield return line;
        }
    }

    private static void ProcessTeamFileLine(
        string line,
        SectionLinesBuilder sectionLinesBuilder,
        ref TeamFileSection currentSection)
    {
        var parsedSectionHeader = TryParseSectionHeader(line);
        if (parsedSectionHeader is not null)
        {
            currentSection = parsedSectionHeader.Value;
            return;
        }

        if (!sectionLinesBuilder.TryAddTeamMemberLine(line, currentSection))
            throw new TeamFileParseException("Team member entries must be inside a valid section.");
    }

    private static TeamFileSection? TryParseSectionHeader(string line)
    {
        if (line == PlayerTeamHeader)
            return TeamFileSection.PlayerTeam;

        if (line == EnemyTeamHeader)
            return TeamFileSection.EnemyTeam;

        return null;
    }

    private static List<TravelerSetup> ParseTravelers(IEnumerable<string> travelerLines)
    {
        var travelers = new List<TravelerSetup>();
        foreach (var travelerLine in travelerLines)
            travelers.Add(ParseTraveler(travelerLine));

        return travelers;
    }

    private static TravelerSetup ParseTraveler(string line)
    {
        var travelerName = ExtractTravelerName(line);
        if (string.IsNullOrWhiteSpace(travelerName))
            throw new TeamFileParseException("Traveler names cannot be empty.");

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

}



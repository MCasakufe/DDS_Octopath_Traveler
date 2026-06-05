namespace Octopath_Traveler_Models.RuntimeData;

public sealed record BeastSkillDefinition(
    string Name,
    double Modifier,
    string Description,
    string Target,
    int Hits);

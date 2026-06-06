namespace Octopath_Traveler_Models.RuntimeData;

public sealed record SkillDefinition(
    string Name,
    int Sp,
    string Description,
    string Type,
    string Target,
    double Modifier,
    string Boost,
    int Hits);

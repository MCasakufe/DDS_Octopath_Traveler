namespace Octopath_Traveler_Models.TeamSelection;

public sealed record TeamSetup(IReadOnlyList<TravelerSetup> Travelers, IReadOnlyList<string> Beasts);

public sealed record TravelerSetup(string Name, IReadOnlyList<string> ActiveSkills, IReadOnlyList<string> PassiveSkills);


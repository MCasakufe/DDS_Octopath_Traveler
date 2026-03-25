using Octopath_Traveler.RuntimeData;
using Octopath_Traveler.TeamSelection;

namespace Octopath_Traveler.Battle;

public sealed class TeamSetupBattleStateFactory
{
    private readonly RuntimeDataCatalogProvider _runtimeDataCatalogProvider;

    public TeamSetupBattleStateFactory(RuntimeDataCatalogProvider runtimeDataCatalogProvider)
    {
        _runtimeDataCatalogProvider = runtimeDataCatalogProvider;
    }

    public BattleState? TryCreate(TeamSetup teamSetup)
    {
        var runtimeDataCatalog = _runtimeDataCatalogProvider.TryLoad();
        if (runtimeDataCatalog is null)
            return null;

        var travelerTeam = TryCreateTravelerTeam(teamSetup, runtimeDataCatalog);
        var beastTeam = TryCreateBeastTeam(teamSetup, runtimeDataCatalog);
        if (travelerTeam is null || beastTeam is null)
            return null;

        return new BattleState(1, travelerTeam, beastTeam);
    }

    private static IReadOnlyList<TravelerCombatUnit>? TryCreateTravelerTeam(TeamSetup teamSetup, RuntimeDataCatalog runtimeDataCatalog)
    {
        var travelerTeam = new List<TravelerCombatUnit>();
        for (var boardSlotIndex = 0; boardSlotIndex < teamSetup.Travelers.Count; boardSlotIndex++)
        {
            var travelerSetup = teamSetup.Travelers[boardSlotIndex];
            if (!runtimeDataCatalog.TravelersByName.TryGetValue(travelerSetup.Name, out var travelerDefinition))
                return null;

            travelerTeam.Add(new TravelerCombatUnit(travelerDefinition, travelerSetup, boardSlotIndex));
        }

        return travelerTeam;
    }

    private static IReadOnlyList<BeastCombatUnit>? TryCreateBeastTeam(TeamSetup teamSetup, RuntimeDataCatalog runtimeDataCatalog)
    {
        var beastTeam = new List<BeastCombatUnit>();
        for (var boardSlotIndex = 0; boardSlotIndex < teamSetup.Beasts.Count; boardSlotIndex++)
        {
            var beastName = teamSetup.Beasts[boardSlotIndex];
            if (!runtimeDataCatalog.BeastsByName.TryGetValue(beastName, out var beastDefinition))
                return null;

            if (!runtimeDataCatalog.BeastSkillNames.Contains(beastDefinition.SkillName))
                return null;

            beastTeam.Add(new BeastCombatUnit(beastDefinition, boardSlotIndex));
        }

        return beastTeam;
    }
}
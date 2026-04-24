using Octopath_Traveler_Models.RuntimeData;
using Octopath_Traveler_Models.TeamSelection;

namespace Octopath_Traveler_Models.Battle;

public sealed class TeamSetupBattleStateFactory
{
    private readonly RuntimeDataCatalogProvider _runtimeDataCatalogProvider;

    public TeamSetupBattleStateFactory(RuntimeDataCatalogProvider runtimeDataCatalogProvider)
    {
        _runtimeDataCatalogProvider = runtimeDataCatalogProvider;
    }

    public BattleState? TryCreate(TeamSetup teamSetup)
    {
        RuntimeDataCatalog runtimeDataCatalog = _runtimeDataCatalogProvider.Load();

        IReadOnlyList<TravelerCombatUnit>? travelerTeam = TryCreateTravelerTeam(teamSetup, runtimeDataCatalog);
        IReadOnlyList<BeastCombatUnit>? beastTeam = TryCreateBeastTeam(teamSetup, runtimeDataCatalog);
        if (travelerTeam is null || beastTeam is null)
            return null;

        return new BattleState(1, travelerTeam, beastTeam);
    }

    private static IReadOnlyList<TravelerCombatUnit>? TryCreateTravelerTeam(TeamSetup teamSetup, RuntimeDataCatalog runtimeDataCatalog)
    {
        List<TravelerCombatUnit> travelerTeam = [];
        for (int boardSlotIndex = 0; boardSlotIndex < teamSetup.Travelers.Count; boardSlotIndex++)
        {
            TravelerSetup travelerSetup = teamSetup.Travelers[boardSlotIndex];
            if (!runtimeDataCatalog.TravelersByName.TryGetValue(travelerSetup.Name, out TravelerDefinition? travelerDefinition)
                || travelerDefinition is null)
                return null;

            travelerTeam.Add(new TravelerCombatUnit(
                travelerDefinition,
                travelerSetup,
                runtimeDataCatalog,
                boardSlotIndex));
        }

        return travelerTeam;
    }

    private static IReadOnlyList<BeastCombatUnit>? TryCreateBeastTeam(TeamSetup teamSetup, RuntimeDataCatalog runtimeDataCatalog)
    {
        List<BeastCombatUnit> beastTeam = [];
        for (int boardSlotIndex = 0; boardSlotIndex < teamSetup.Beasts.Count; boardSlotIndex++)
        {
            string beastName = teamSetup.Beasts[boardSlotIndex];
            if (!runtimeDataCatalog.BeastsByName.TryGetValue(beastName, out BeastDefinition? beastDefinition)
                || beastDefinition is null)
                return null;

            if (!runtimeDataCatalog.TryGetBeastSkill(beastDefinition.SkillName, out BeastSkillDefinition? skillDefinition)
                || skillDefinition is null)
                return null;

            beastTeam.Add(new BeastCombatUnit(beastDefinition, skillDefinition, boardSlotIndex));
        }

        return beastTeam;
    }
}


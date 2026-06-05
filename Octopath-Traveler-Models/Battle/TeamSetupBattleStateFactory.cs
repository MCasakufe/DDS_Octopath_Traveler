using Octopath_Traveler_Models.RuntimeData;
using Octopath_Traveler_Models.TeamSelection;

namespace Octopath_Traveler_Models.Battle;

public sealed class TeamSetupBattleStateFactory
{
    private const int InitialBattleRoundNumber = 1;
    private const int FirstBoardSlotIndex = 0;
    private const int IntimidationDurationRounds = 2;

    private readonly RuntimeDataCatalogProvider _runtimeDataCatalogProvider;
    private readonly PassiveSkillProfileFactory _passiveSkillProfileFactory = new();

    public TeamSetupBattleStateFactory(RuntimeDataCatalogProvider runtimeDataCatalogProvider)
    {
        _runtimeDataCatalogProvider = runtimeDataCatalogProvider;
    }

    public BattleState Create(TeamSetup teamSetup)
    {
        RuntimeDataCatalog runtimeDataCatalog = _runtimeDataCatalogProvider.Load();

        IReadOnlyList<TravelerCombatUnit> travelerTeam = CreateTravelerTeam(teamSetup, runtimeDataCatalog);
        IReadOnlyList<BeastCombatUnit> beastTeam = CreateBeastTeam(teamSetup, runtimeDataCatalog);
        ApplyIntimidationIfNeeded(travelerTeam, beastTeam);
        return new BattleState(InitialBattleRoundNumber, travelerTeam, beastTeam);
    }

    private IReadOnlyList<TravelerCombatUnit> CreateTravelerTeam(
        TeamSetup teamSetup,
        RuntimeDataCatalog runtimeDataCatalog)
    {
        List<TravelerCombatUnit> travelerTeam = [];
        for (int boardSlotIndex = FirstBoardSlotIndex; boardSlotIndex < teamSetup.Travelers.Count; boardSlotIndex++)
        {
            TravelerSetup travelerSetup = teamSetup.Travelers[boardSlotIndex];
            if (!runtimeDataCatalog.TravelersByName.TryGetValue(travelerSetup.Name, out TravelerDefinition? travelerDefinition)
                || travelerDefinition is null)
                throw new BattleStateCreationException($"Unknown traveler definition '{travelerSetup.Name}'.");

            travelerTeam.Add(CreateTravelerCombatUnit(
                travelerDefinition,
                travelerSetup,
                runtimeDataCatalog,
                boardSlotIndex));
        }

        return travelerTeam;
    }

    private TravelerCombatUnit CreateTravelerCombatUnit(
        TravelerDefinition travelerDefinition,
        TravelerSetup travelerSetup,
        RuntimeDataCatalog runtimeDataCatalog,
        int boardSlotIndex)
    {
        PassiveSkillProfile passiveSkillProfile = _passiveSkillProfileFactory.Create(travelerSetup.PassiveSkills);
        return new TravelerCombatUnit(
            travelerDefinition,
            travelerSetup,
            runtimeDataCatalog,
            boardSlotIndex,
            passiveSkillProfile);
    }

    private static IReadOnlyList<BeastCombatUnit> CreateBeastTeam(TeamSetup teamSetup, RuntimeDataCatalog runtimeDataCatalog)
    {
        List<BeastCombatUnit> beastTeam = [];
        for (int boardSlotIndex = FirstBoardSlotIndex; boardSlotIndex < teamSetup.Beasts.Count; boardSlotIndex++)
        {
            string beastName = teamSetup.Beasts[boardSlotIndex];
            if (!runtimeDataCatalog.BeastsByName.TryGetValue(beastName, out BeastDefinition? beastDefinition)
                || beastDefinition is null)
                throw new BattleStateCreationException($"Unknown beast definition '{beastName}'.");

            BeastSkillDefinition? skillDefinition = runtimeDataCatalog.SelectBeastSkillOrNull(beastDefinition.SkillName);
            if (skillDefinition is null)
                throw new BattleStateCreationException($"Unknown beast skill definition '{beastDefinition.SkillName}'.");

            beastTeam.Add(new BeastCombatUnit(beastDefinition, skillDefinition, boardSlotIndex));
        }

        return beastTeam;
    }

    private static void ApplyIntimidationIfNeeded(
        IReadOnlyList<TravelerCombatUnit> travelerTeam,
        IReadOnlyList<BeastCombatUnit> beastTeam)
    {
        if (!CanApplyIntimidation(travelerTeam))
            return;

        foreach (BeastCombatUnit beast in beastTeam)
            ApplyIntimidationStatuses(beast);
    }

    private static bool CanApplyIntimidation(IReadOnlyList<TravelerCombatUnit> travelerTeam)
        => travelerTeam.Count > FirstBoardSlotIndex
           && travelerTeam.Any(traveler => traveler.HasIntimidation)
           && IsEven(travelerTeam[FirstBoardSlotIndex].CurrentHp)
           && IsEven(travelerTeam[FirstBoardSlotIndex].CurrentSp);

    private static void ApplyIntimidationStatuses(BeastCombatUnit beast)
    {
        beast.ApplyStatusEffect(UnitStatusEffectKind.DecreasedPhysicalDefense, IntimidationDurationRounds);
        beast.ApplyStatusEffect(UnitStatusEffectKind.DecreasedElementalDefense, IntimidationDurationRounds);
    }

    private static bool IsEven(int value)
        => value % 2 == 0;
}


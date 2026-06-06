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
            TravelerDefinition travelerDefinition = FindTravelerDefinition(runtimeDataCatalog, travelerSetup.Name);

            travelerTeam.Add(CreateTravelerCombatUnit(
                travelerDefinition,
                travelerSetup,
                runtimeDataCatalog,
                boardSlotIndex));
        }

        return travelerTeam;
    }

    private static TravelerDefinition FindTravelerDefinition(
        RuntimeDataCatalog runtimeDataCatalog,
        string travelerName)
    {
        if (runtimeDataCatalog.TravelersByName.TryGetValue(
                travelerName,
                out TravelerDefinition? travelerDefinition)
            && travelerDefinition is not null)
        {
            return travelerDefinition;
        }

        throw new BattleStateCreationException($"Unknown traveler definition '{travelerName}'.");
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

    private static IReadOnlyList<BeastCombatUnit> CreateBeastTeam(
        TeamSetup teamSetup,
        RuntimeDataCatalog runtimeDataCatalog)
    {
        List<BeastCombatUnit> beastTeam = [];
        for (int boardSlotIndex = FirstBoardSlotIndex; boardSlotIndex < teamSetup.Beasts.Count; boardSlotIndex++)
        {
            string beastName = teamSetup.Beasts[boardSlotIndex];
            BeastDefinition beastDefinition = FindBeastDefinition(runtimeDataCatalog, beastName);
            BeastSkillDefinition skillDefinition = FindBeastSkillDefinition(runtimeDataCatalog, beastDefinition);

            beastTeam.Add(new BeastCombatUnit(beastDefinition, skillDefinition, boardSlotIndex));
        }

        return beastTeam;
    }

    private static BeastDefinition FindBeastDefinition(RuntimeDataCatalog runtimeDataCatalog, string beastName)
    {
        if (runtimeDataCatalog.BeastsByName.TryGetValue(beastName, out BeastDefinition? beastDefinition)
            && beastDefinition is not null)
        {
            return beastDefinition;
        }

        throw new BattleStateCreationException($"Unknown beast definition '{beastName}'.");
    }

    private static BeastSkillDefinition FindBeastSkillDefinition(
        RuntimeDataCatalog runtimeDataCatalog,
        BeastDefinition beastDefinition)
    {
        BeastSkillDefinition? skillDefinition = runtimeDataCatalog.SelectBeastSkillOrNull(beastDefinition.SkillName);
        if (skillDefinition is not null)
            return skillDefinition;

        throw new BattleStateCreationException($"Unknown beast skill definition '{beastDefinition.SkillName}'.");
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
           && HasEvenHpAndSp(travelerTeam[FirstBoardSlotIndex]);

    private static bool HasEvenHpAndSp(TravelerCombatUnit traveler)
        => IsEven(traveler.CurrentHp) && IsEven(traveler.CurrentSp);

    private static void ApplyIntimidationStatuses(BeastCombatUnit beast)
    {
        beast.ApplyStatusEffect(UnitStatusEffectKind.DecreasedPhysicalDefense, IntimidationDurationRounds);
        beast.ApplyStatusEffect(UnitStatusEffectKind.DecreasedElementalDefense, IntimidationDurationRounds);
    }

    private static bool IsEven(int value)
        => value % 2 == 0;
}


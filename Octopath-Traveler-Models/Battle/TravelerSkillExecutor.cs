using Octopath_Traveler_Models.RuntimeData;

namespace Octopath_Traveler_Models.Battle;

public sealed record TravelerSkillAction(
    string TravelerName,
    string SkillName,
    IReadOnlyList<TravelerSkillResult> Results);

public sealed record TravelerSkillExecutionRequest(
    TravelerCombatUnit Traveler,
    BattleState BattleState,
    TravelerTurnOutcome TurnOutcome,
    string SkillName);

public sealed class TravelerSkillExecutor
{
    private const string SingleTargetType = "Single";
    private const string EnemiesTargetType = "Enemies";
    private const int LegholdTrapDurationRounds = 2;
    private const int ReviveStartingHp = 1;
    private const double LastStandBaseMultiplier = 1.0;
    private const double LastStandMissingHpMultiplierPerPercent = 0.03;

    private static readonly string[] ShootingStarsDamageTypes = ["Wind", "Light", "Dark"];
    private static readonly TravelerSkillTargetSelector OneBeastTargetSelector = new OneBeastTravelerSkillTargetSelector();
    private static readonly TravelerSkillTargetSelector AllAliveBeastsTargetSelector =
        new AllAliveBeastsTravelerSkillTargetSelector();
    private static readonly TravelerSkillTargetSelector OneTravelerTargetSelector =
        new OneTravelerTravelerSkillTargetSelector();
    private static readonly TravelerSkillTargetSelector AllAliveTravelersTargetSelector =
        new AllAliveTravelersTravelerSkillTargetSelector();
    private static readonly TravelerSkillTargetSelector AllDefeatedTravelersTargetSelector =
        new AllDefeatedTravelersTravelerSkillTargetSelector();
    private static readonly TravelerSkillTargetSelector NoTargetSelector = new NoTravelerSkillTargetSelector();

    private static readonly HashSet<string> PartyHealingSkills = new(StringComparer.Ordinal)
    {
        "Heal Wounds",
        "Heal More"
    };

    private static readonly HashSet<string> BasicOffensiveSkills = new(StringComparer.Ordinal)
    {
        "Fireball",
        "Icewind",
        "Lightning Bolt",
        "Holy Light",
        "Luminescence",
        "Tradewinds",
        "Trade Tempest",
        "Level Slash",
        "Cross Strike",
        "Moonlight Waltz",
        "Night Ode",
        "Icicle",
        "Amputation",
        "Wildfire",
        "True Strike",
        "Thunderbird",
        "Tiger Rage",
        "Qilin's Horn",
        "Yatagarasu",
        "Fox Spirit",
        "Phoenix Storm"
    };

    private static readonly IReadOnlyDictionary<string, TravelerSkillTargetSelector> StandardOffensiveTargetSelectors =
        new Dictionary<string, TravelerSkillTargetSelector>(StringComparer.Ordinal)
        {
            [SingleTargetType] = OneBeastTargetSelector,
            [EnemiesTargetType] = AllAliveBeastsTargetSelector
        };

    private static readonly TravelerSkillTargetSelector StandardOffensiveTargetSelector =
        new SkillTargetTypeTravelerSkillTargetSelector(StandardOffensiveTargetSelectors, NoTargetSelector);

    private static readonly IReadOnlyList<TravelerSkillBehavior> SkillBehaviors = BuildSkillBehaviors();

    public TravelerSkillAction ExecuteSkill(TravelerSkillExecutionRequest skillExecutionRequest)
    {
        TravelerCombatUnit traveler = skillExecutionRequest.Traveler;
        string skillName = skillExecutionRequest.SkillName;

        SkillDefinition? selectedSkill = SelectCastableSkill(traveler, skillName);
        if (selectedSkill is null)
            return new TravelerSkillAction(traveler.Name, skillName, []);

        ConsumeSkillSp(traveler, selectedSkill);

        IReadOnlyList<TravelerSkillResult> results = ExecuteSelectedSkill(skillExecutionRequest, selectedSkill);

        return new TravelerSkillAction(traveler.Name, skillName, results);
    }

    private static SkillDefinition? SelectCastableSkill(TravelerCombatUnit traveler, string skillName)
    {
        SkillDefinition? selectedSkill = traveler.AssignedActiveSkills
            .FirstOrDefault(activeSkill => activeSkill.Name == skillName);
        return IsCastableSkill(traveler, selectedSkill) ? selectedSkill : null;
    }

    private static bool IsCastableSkill(TravelerCombatUnit traveler, SkillDefinition? selectedSkill)
        => selectedSkill is not null && traveler.CurrentSp >= selectedSkill.Sp;

    private static void ConsumeSkillSp(TravelerCombatUnit traveler, SkillDefinition selectedSkill)
        => traveler.ConsumeSkillSp(selectedSkill.Sp);

    private static IReadOnlyList<TravelerSkillResult> ExecuteSelectedSkill(
        TravelerSkillExecutionRequest skillExecutionRequest,
        SkillDefinition selectedSkill)
    {
        TravelerSkillExecutionContext executionContext = new(skillExecutionRequest, selectedSkill);
        TravelerSkillBehavior? skillBehavior = SelectSkillBehavior(executionContext.SkillName);
        return skillBehavior is null ? [] : skillBehavior.Execute(executionContext);
    }

    private static IReadOnlyList<TravelerSkillBehavior> BuildSkillBehaviors()
        =>
        [
            new ExactNameTravelerSkillBehavior(
                "Leghold Trap",
                OneBeastTargetSelector,
                new DecreaseBeastPriorityTravelerSkillEffect(LegholdTrapDurationRounds)),
            new ExactNameTravelerSkillBehavior(
                "Spearhead",
                OneBeastTargetSelector,
                new QueueTravelerPriorityTravelerSkillEffect(),
                new BeastDamageTravelerSkillEffect()),
            new ExactNameTravelerSkillBehavior(
                "First Aid",
                OneTravelerTargetSelector,
                new TravelerHealingSkillEffect()),
            new ExactNameTravelerSkillBehavior(
                "Vivify",
                OneTravelerTargetSelector,
                new ReviveSelectedTravelerSkillEffect(ReviveStartingHp),
                new TravelerHealingSkillEffect()),
            new ExactNameTravelerSkillBehavior(
                "Revive",
                AllDefeatedTravelersTargetSelector,
                new ReviveTravelersSkillEffect(ReviveStartingHp)),
            new ExactNameTravelerSkillBehavior(
                "Shooting Stars",
                AllAliveBeastsTargetSelector,
                new ShootingStarsTravelerSkillEffect(ShootingStarsDamageTypes)),
            new ExactNameTravelerSkillBehavior(
                "Nightmare Chimera",
                OneBeastTargetSelector,
                new SelectedWeaponBeastDamageTravelerSkillEffect()),
            new ExactNameTravelerSkillBehavior(
                "Last Stand",
                AllAliveBeastsTargetSelector,
                new LastStandTravelerSkillEffect(
                    LastStandBaseMultiplier,
                    LastStandMissingHpMultiplierPerPercent)),
            new ExactNameTravelerSkillBehavior(
                "Mercy Strike",
                OneBeastTargetSelector,
                new MercyStrikeTravelerSkillEffect()),
            new SkillNameSetTravelerSkillBehavior(
                PartyHealingSkills,
                AllAliveTravelersTargetSelector,
                new TravelerHealingSkillEffect()),
            new SkillNameSetTravelerSkillBehavior(
                BasicOffensiveSkills,
                StandardOffensiveTargetSelector,
                new BeastDamageTravelerSkillEffect())
        ];

    private static TravelerSkillBehavior? SelectSkillBehavior(string skillName)
        => SkillBehaviors.FirstOrDefault(skillBehavior => skillBehavior.CanExecuteSkill(skillName));
}

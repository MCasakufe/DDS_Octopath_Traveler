using Octopath_Traveler_Models.RuntimeData;

namespace Octopath_Traveler_Models.Battle;

public sealed class TravelerSkillExecutor
{
    private const string SingleTargetType = "Single";
    private const string EnemiesTargetType = "Enemies";
    private const int LegholdTrapDurationRounds = 2;
    private const int StandardStatusDurationRounds = 2;
    private const int StoutWallDurationRounds = 3;
    private const int ReviveStartingHp = 1;
    private const double LastStandBaseMultiplier = 1.0;
    private const double LastStandMissingHpMultiplierPerPercent = 0.03;

    private static readonly string[] ShootingStarsDamageTypes = ["Wind", "Light", "Dark"];
    private static readonly string[] BalogarsBladeDamageTypes = ["Fire", "Ice", "Lightning", "Wind", "Light", "Dark"];
    private static readonly string[] WinnehildsBattleCryDamageTypes = ["Sword", "Spear", "Dagger", "Axe", "Bow", "Stave"];
    private static readonly UnitStatusEffectKind[] StarsongStatusEffects =
    [
        UnitStatusEffectKind.IncreasedPhysicalDefense,
        UnitStatusEffectKind.IncreasedElementalDefense,
        UnitStatusEffectKind.IncreasedSpeed
    ];

    private static readonly TravelerSkillTargetSelector OneBeastTargetSelector = new OneBeastTravelerSkillTargetSelector();
    private static readonly TravelerSkillTargetSelector AllAliveBeastsTargetSelector =
        new AllAliveBeastsTravelerSkillTargetSelector();
    private static readonly TravelerSkillTargetSelector OneTravelerTargetSelector =
        new OneTravelerTravelerSkillTargetSelector();
    private static readonly TravelerSkillTargetSelector UserTargetSelector =
        new UserTravelerSkillTargetSelector();
    private static readonly TravelerSkillTargetSelector AllAliveTravelersTargetSelector =
        new AllAliveTravelersTravelerSkillTargetSelector();
    private static readonly TravelerSkillTargetSelector AllDefeatedTravelersTargetSelector =
        new AllDefeatedTravelersTravelerSkillTargetSelector();
    private static readonly TravelerSkillTargetSelector NoTargetSelector = new NoTravelerSkillTargetSelector();
    private static readonly TravelerSkillTargetSelector LowestPhysDefBeastTargetSelector =
        new LowestPhysDefBeastTravelerSkillTargetSelector();
    private static readonly TravelerSkillTargetSelector LowestCurrentHpBeastTargetSelector =
        new LowestCurrentHpBeastTravelerSkillTargetSelector();
    private static readonly TravelerSkillTargetSelector HighestSpeedBeastTargetSelector =
        new HighestSpeedBeastTravelerSkillTargetSelector();

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
        "Phoenix Storm",
        "Fire Storm",
        "Blizzard",
        "Lightning Blast",
        "Ignis Ardere",
        "Glacies Claudere",
        "Tonitrus Canere",
        "Ventus Saltare",
        "Lux Congerere",
        "Tenebrae Operire",
        "Arrowstorm"
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

        SkillDefinition? selectedSkill = SelectCastableSkill(skillExecutionRequest);
        if (selectedSkill is null)
            return new TravelerSkillAction(traveler.Name, skillName, []);

        ConsumeSkillSp(traveler, selectedSkill);

        IReadOnlyList<TravelerSkillResult> results = ApplySelectedSkillBehavior(skillExecutionRequest, selectedSkill);

        return new TravelerSkillAction(traveler.Name, skillName, results);
    }

    private static SkillDefinition? SelectCastableSkill(TravelerSkillExecutionRequest skillExecutionRequest)
    {
        SkillDefinition? selectedSkill = skillExecutionRequest.Traveler.AssignedActiveSkills
            .FirstOrDefault(activeSkill => activeSkill.Name == skillExecutionRequest.SkillName);
        return IsCastableSkill(skillExecutionRequest, selectedSkill) ? selectedSkill : null;
    }

    private static bool IsCastableSkill(
        TravelerSkillExecutionRequest skillExecutionRequest,
        SkillDefinition? selectedSkill)
        => selectedSkill is not null
           && skillExecutionRequest.Traveler.CurrentSp >= skillExecutionRequest.Traveler.CalculateSkillSpCost(selectedSkill.Sp)
           && HasRequiredBp(skillExecutionRequest, selectedSkill);

    private static bool HasRequiredBp(
        TravelerSkillExecutionRequest skillExecutionRequest,
        SkillDefinition selectedSkill)
        => !TravelerDivineSkillCatalog.IsDivineSkill(selectedSkill)
           || (skillExecutionRequest.Traveler.CurrentBp >= TravelerDivineSkillCatalog.RequiredBpCost
               && skillExecutionRequest.TurnOutcome.UsedBp == TravelerDivineSkillCatalog.RequiredBpCost);

    private static void ConsumeSkillSp(TravelerCombatUnit traveler, SkillDefinition selectedSkill)
        => traveler.ConsumeSkillSp(traveler.CalculateSkillSpCost(selectedSkill.Sp));

    private static IReadOnlyList<TravelerSkillResult> ApplySelectedSkillBehavior(
        TravelerSkillExecutionRequest skillExecutionRequest,
        SkillDefinition selectedSkill)
    {
        TravelerSkillExecutionContext executionContext = new(skillExecutionRequest, selectedSkill);
        TravelerSkillBehavior? skillBehavior = SelectSkillBehavior(executionContext.SkillName);
        return skillBehavior is null ? [] : skillBehavior.Apply(executionContext);
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
            new ExactNameTravelerSkillBehavior(
                "HP Thief",
                OneBeastTargetSelector,
                new HpThiefTravelerSkillEffect()),
            new ExactNameTravelerSkillBehavior(
                "Steal SP",
                OneBeastTargetSelector,
                new StealSpTravelerSkillEffect()),
            new ExactNameTravelerSkillBehavior(
                "Thousand Spears",
                LowestPhysDefBeastTargetSelector,
                new BeastDamageTravelerSkillEffect()),
            new ExactNameTravelerSkillBehavior(
                "Rain of Arrows",
                LowestCurrentHpBeastTargetSelector,
                new BeastDamageTravelerSkillEffect()),
            new ExactNameTravelerSkillBehavior(
                "Guardian Liondog",
                HighestSpeedBeastTargetSelector,
                new BeastDamageTravelerSkillEffect()),
            new ExactNameTravelerSkillBehavior(
                "Sheltering Veil",
                OneTravelerTargetSelector,
                new TravelerStatusEffectSkillEffect(
                    UnitStatusEffectKind.IncreasedElementalDefense,
                    StandardStatusDurationRounds)),
            new ExactNameTravelerSkillBehavior(
                "Abide",
                OneTravelerTargetSelector,
                new TravelerStatusEffectSkillEffect(
                    UnitStatusEffectKind.IncreasedPhysicalAttack,
                    StandardStatusDurationRounds)),
            new ExactNameTravelerSkillBehavior(
                "Stout Wall",
                UserTargetSelector,
                new TravelerStatusEffectSkillEffect(
                    UnitStatusEffectKind.IncreasedPhysicalDefense,
                    StoutWallDurationRounds)),
            new ExactNameTravelerSkillBehavior(
                "Lion Dance",
                OneTravelerTargetSelector,
                new TravelerStatusEffectSkillEffect(
                    UnitStatusEffectKind.IncreasedPhysicalAttack,
                    StandardStatusDurationRounds)),
            new ExactNameTravelerSkillBehavior(
                "Peacock Strut",
                OneTravelerTargetSelector,
                new TravelerStatusEffectSkillEffect(
                    UnitStatusEffectKind.IncreasedElementalAttack,
                    StandardStatusDurationRounds)),
            new ExactNameTravelerSkillBehavior(
                "Mole Dance",
                OneTravelerTargetSelector,
                new TravelerStatusEffectSkillEffect(
                    UnitStatusEffectKind.IncreasedPhysicalDefense,
                    StandardStatusDurationRounds)),
            new ExactNameTravelerSkillBehavior(
                "Panther Dance",
                OneTravelerTargetSelector,
                new TravelerStatusEffectSkillEffect(
                    UnitStatusEffectKind.IncreasedSpeed,
                    StandardStatusDurationRounds)),
            new ExactNameTravelerSkillBehavior(
                "Shackle Foe",
                OneBeastTargetSelector,
                new TravelerStatusEffectSkillEffect(
                    UnitStatusEffectKind.DecreasedPhysicalAttack,
                    StandardStatusDurationRounds)),
            new ExactNameTravelerSkillBehavior(
                "Armor Corrosive",
                OneBeastTargetSelector,
                new TravelerStatusEffectSkillEffect(
                    UnitStatusEffectKind.DecreasedPhysicalDefense,
                    StandardStatusDurationRounds)),
            new ExactNameTravelerSkillBehavior(
                "Starsong",
                OneTravelerTargetSelector,
                new TravelerStatusEffectSkillEffect(
                    StarsongStatusEffects,
                    StandardStatusDurationRounds)),
            new ExactNameTravelerSkillBehavior(
                "Elemental Break",
                OneBeastTargetSelector,
                new ElementalBreakTravelerSkillEffect()),
            new ExactNameTravelerSkillBehavior(
                TravelerDivineSkillCatalog.BrandsThunderName,
                OneBeastTargetSelector,
                new BeastDamageTravelerSkillEffect()),
            new ExactNameTravelerSkillBehavior(
                TravelerDivineSkillCatalog.DraefendisRageName,
                AllAliveBeastsTargetSelector,
                new BeastDamageTravelerSkillEffect()),
            new ExactNameTravelerSkillBehavior(
                TravelerDivineSkillCatalog.SteorrasProphecyName,
                AllAliveBeastsTargetSelector,
                new SteorrasProphecyTravelerSkillEffect()),
            new ExactNameTravelerSkillBehavior(
                TravelerDivineSkillCatalog.BalogarsBladeName,
                OneBeastTargetSelector,
                new OrderedDamageTypesTravelerSkillEffect(BalogarsBladeDamageTypes)),
            new ExactNameTravelerSkillBehavior(
                TravelerDivineSkillCatalog.WinnehildsBattleCryName,
                AllAliveBeastsTargetSelector,
                new OrderedDamageTypesTravelerSkillEffect(WinnehildsBattleCryDamageTypes)),
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
        => SkillBehaviors.FirstOrDefault(skillBehavior => skillBehavior.CanHandleSkill(skillName));
}

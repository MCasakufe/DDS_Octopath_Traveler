namespace Octopath_Traveler_Models.Battle;

public abstract class Unit
{
    private const int NoHpRemaining = 0;
    private const int NoRounds = 0;
    private const int RoundCountdownStep = 1;
    private const double NoStatusDamageMultiplier = 1.0;
    private const double IncreasedStatusDamageMultiplier = 1.5;
    private const double DecreasedStatusDamageMultiplier = 2.0 / 3.0;
    private const double IncreasedSpeedMultiplier = 1.5;
    private const int PassiveDurationBonusRounds = 1;

    private readonly Dictionary<UnitStatusEffectKind, int> _statusEffectDurations = new();
    private readonly HashSet<UnitStatusEffectKind> _permanentStatusEffects = new();

    protected Unit(
        string name,
        int maxHp,
        int physAtk,
        int physDef,
        int elemAtk,
        int elemDef,
        int speed,
        int boardSlotIndex)
    {
        Name = name;
        MaxHp = maxHp;
        CurrentHp = maxHp;
        PhysAtk = physAtk;
        PhysDef = physDef;
        ElemAtk = elemAtk;
        ElemDef = elemDef;
        Speed = speed;
        BoardSlotIndex = boardSlotIndex;
    }

    public string Name { get; }

    public int MaxHp { get; }

    public int CurrentHp { get; set; }

    public int PhysAtk { get; }

    public int PhysDef { get; }

    public int ElemAtk { get; }

    public int ElemDef { get; }

    public int Speed { get; }

    public int BoardSlotIndex { get; }

    public bool IsAlive => CurrentHp > NoHpRemaining;

    public IReadOnlyList<UnitStatusEffect> ActiveStatusEffects => _statusEffectDurations
        .OrderBy(effectDuration => effectDuration.Key)
        .Select(effectDuration => new UnitStatusEffect(effectDuration.Key, effectDuration.Value))
        .Concat(_permanentStatusEffects
            .OrderBy(statusEffectKind => statusEffectKind)
            .Select(statusEffectKind => new UnitStatusEffect(statusEffectKind, NoRounds)))
        .ToList();

    public void ApplyStatusEffect(UnitStatusEffectKind statusEffectKind, int durationRounds)
    {
        if (durationRounds <= NoRounds)
            return;

        if (_statusEffectDurations.ContainsKey(statusEffectKind))
            _statusEffectDurations[statusEffectKind] += durationRounds;
        else
            _statusEffectDurations[statusEffectKind] = durationRounds;
    }

    public void ApplyPermanentStatusEffect(UnitStatusEffectKind statusEffectKind)
        => _permanentStatusEffects.Add(statusEffectKind);

    public void ApplyStatusEffect(
        UnitStatusEffectKind statusEffectKind,
        int durationRounds,
        TravelerCombatUnit? source)
        => ApplyStatusEffect(statusEffectKind, CalculatePassiveAdjustedDuration(
            statusEffectKind,
            durationRounds,
            source));

    public int ApplyStatusEffectAndReturnDuration(
        UnitStatusEffectKind statusEffectKind,
        int durationRounds,
        TravelerCombatUnit? source)
    {
        int adjustedDuration = CalculatePassiveAdjustedDuration(statusEffectKind, durationRounds, source);
        ApplyStatusEffect(statusEffectKind, adjustedDuration);
        return adjustedDuration;
    }

    public bool HasStatusEffect(UnitStatusEffectKind statusEffectKind)
        => _statusEffectDurations.ContainsKey(statusEffectKind)
            || _permanentStatusEffects.Contains(statusEffectKind);

    public int GetStatusEffectDuration(UnitStatusEffectKind statusEffectKind)
        => _permanentStatusEffects.Contains(statusEffectKind)
            ? NoRounds
            : _statusEffectDurations.GetValueOrDefault(statusEffectKind);

    public double GetPhysicalAttackDamageMultiplier()
        => CalculateStatusDamageMultiplier(
            UnitStatusEffectKind.IncreasedPhysicalAttack,
            UnitStatusEffectKind.DecreasedPhysicalAttack);

    public double GetElementalAttackDamageMultiplier()
        => HasStatusEffect(UnitStatusEffectKind.IncreasedElementalAttack)
            ? IncreasedStatusDamageMultiplier
            : NoStatusDamageMultiplier;

    public double GetPhysicalDefenseDamageMultiplier()
        => CalculateStatusDamageMultiplier(
            UnitStatusEffectKind.DecreasedPhysicalDefense,
            UnitStatusEffectKind.IncreasedPhysicalDefense);

    public double GetElementalDefenseDamageMultiplier()
        => CalculateStatusDamageMultiplier(
            UnitStatusEffectKind.DecreasedElementalDefense,
            UnitStatusEffectKind.IncreasedElementalDefense);

    public int GetEffectiveSpeed()
        => CalculateEffectiveSpeed(HasStatusEffect(UnitStatusEffectKind.IncreasedSpeed));

    public int GetEffectiveSpeedAfterRoundCountdown()
        => CalculateEffectiveSpeed(
            GetStatusEffectDuration(UnitStatusEffectKind.IncreasedSpeed) > RoundCountdownStep);

    protected void DecreaseStatusEffectDurationsForNextRound()
    {
        foreach (UnitStatusEffectKind statusEffectKind in _statusEffectDurations.Keys.ToList())
            DecreaseStatusEffectDuration(statusEffectKind);
    }

    private double CalculateStatusDamageMultiplier(
        UnitStatusEffectKind increasedDamageStatus,
        UnitStatusEffectKind decreasedDamageStatus)
    {
        double multiplier = NoStatusDamageMultiplier;
        if (HasStatusEffect(increasedDamageStatus))
            multiplier *= IncreasedStatusDamageMultiplier;

        if (HasStatusEffect(decreasedDamageStatus))
            multiplier *= DecreasedStatusDamageMultiplier;

        return multiplier;
    }

    private int CalculateEffectiveSpeed(bool hasIncreasedSpeed)
        => hasIncreasedSpeed
            ? (int)Math.Floor(Speed * IncreasedSpeedMultiplier)
            : Speed;

    private int CalculatePassiveAdjustedDuration(
        UnitStatusEffectKind statusEffectKind,
        int durationRounds,
        TravelerCombatUnit? source)
    {
        int adjustedDuration = durationRounds;
        if (IsBuffStatus(statusEffectKind) && HasPersistence)
            adjustedDuration += PassiveDurationBonusRounds;

        if (IsBuffStatus(statusEffectKind) && source is not null && source.HasTheShowGoesOn)
            adjustedDuration += PassiveDurationBonusRounds;

        return adjustedDuration;
    }

    private bool HasPersistence
        => this is TravelerCombatUnit traveler && traveler.HasPersistence;

    private static bool IsBuffStatus(UnitStatusEffectKind statusEffectKind)
        => statusEffectKind is UnitStatusEffectKind.IncreasedPhysicalAttack
            or UnitStatusEffectKind.IncreasedPhysicalDefense
            or UnitStatusEffectKind.IncreasedElementalAttack
            or UnitStatusEffectKind.IncreasedElementalDefense
            or UnitStatusEffectKind.IncreasedSpeed;

    private void DecreaseStatusEffectDuration(UnitStatusEffectKind statusEffectKind)
    {
        int remainingRounds = _statusEffectDurations[statusEffectKind] - RoundCountdownStep;
        if (remainingRounds <= NoRounds)
            _statusEffectDurations.Remove(statusEffectKind);
        else
            _statusEffectDurations[statusEffectKind] = remainingRounds;
    }
}

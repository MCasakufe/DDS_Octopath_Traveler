namespace Octopath_Traveler_Models.Battle;

internal sealed class TravelerDamageApplier
{
    private const int NoDamage = 0;
    private const int MinimumSurvivingHp = 1;
    private const int HangToughHpThresholdDivisor = 10;

    public TravelerDamageApplicationResult ApplyDamage(TravelerDamageApplication damageApplication)
    {
        int displayedDamage = CalculateDisplayedDamage(damageApplication);
        int appliedDamage = CalculateAppliedDamage(damageApplication);
        damageApplication.Target.ReceiveDamage(appliedDamage);
        bool revivedByEncore = TryApplyEncore(damageApplication.Target);
        return new TravelerDamageApplicationResult(displayedDamage, revivedByEncore);
    }

    private static int CalculateDisplayedDamage(TravelerDamageApplication damageApplication)
    {
        if (IsNegatedByDivineAura(damageApplication))
            return NoDamage;

        if (IsStoppedByHangTough(damageApplication))
            return damageApplication.Target.CurrentHp - MinimumSurvivingHp;

        return damageApplication.Damage;
    }

    private static int CalculateAppliedDamage(TravelerDamageApplication damageApplication)
    {
        if (IsNegatedByDivineAura(damageApplication))
            return NoDamage;

        if (IsStoppedByHangTough(damageApplication))
            return damageApplication.Target.CurrentHp - MinimumSurvivingHp;

        return damageApplication.Damage;
    }

    private static bool IsNegatedByDivineAura(TravelerDamageApplication damageApplication)
        => damageApplication.Target.HasDivineAura
           && IsEven(damageApplication.Target.CurrentHp)
           && IsEven(damageApplication.Attacker.CurrentHp);

    private static bool IsStoppedByHangTough(TravelerDamageApplication damageApplication)
        => damageApplication.Target.HasHangTough
           && damageApplication.Target.CurrentHp > damageApplication.Target.MaxHp / HangToughHpThresholdDivisor
           && damageApplication.Damage >= damageApplication.Target.CurrentHp;

    private static bool TryApplyEncore(TravelerCombatUnit target)
    {
        if (!CanTriggerEncore(target))
            return false;

        target.TriggerEncoreRevive();
        return true;
    }

    private static bool CanTriggerEncore(TravelerCombatUnit target)
        => target.HasEncore && !target.HasTriggeredEncore && !target.IsAlive;

    private static bool IsEven(int value)
        => value % 2 == 0;
}

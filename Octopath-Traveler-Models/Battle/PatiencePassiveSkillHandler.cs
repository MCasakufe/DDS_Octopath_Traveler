namespace Octopath_Traveler_Models.Battle;

internal sealed class PatiencePassiveSkillHandler
    : TravelerPassiveSkillHandler, ExtraTurnPassiveSkillHandler
{
    private const int EvenNumberRemainder = 0;
    private const int EvenNumberDivisor = 2;

    public PatiencePassiveSkillHandler(TravelerCombatUnit traveler)
        : base(traveler)
    {
    }

    public bool CanGrantExtraTurn(PassiveExtraTurnEligibilityContext context)
        => Traveler.IsAlive
           && !context.TravelersWithGrantedExtraTurn.Contains(Traveler.BoardSlotIndex)
           && HasEvenCurrentHpAndSp();

    private bool HasEvenCurrentHpAndSp()
        => IsEven(Traveler.CurrentHp) && IsEven(Traveler.CurrentSp);

    private static bool IsEven(int value)
        => value % EvenNumberDivisor == EvenNumberRemainder;
}

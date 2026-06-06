namespace Octopath_Traveler_Models.Battle;

internal sealed class VimAndVigorPassiveSkillHandler
    : TravelerPassiveSkillHandler, RoundEndPassiveSkillHandler
{
    private const int HealingDivisor = 10;

    public VimAndVigorPassiveSkillHandler(TravelerCombatUnit traveler)
        : base(traveler)
    {
    }

    public void Handle(RoundEndPassiveRecoveryContext context)
    {
        if (!context.Traveler.IsAlive)
            return;

        int healingAmount = context.Traveler.CalculateReceivedHealing(context.Traveler.MaxHp / HealingDivisor);
        context.Traveler.RecoverHp(healingAmount);
    }
}

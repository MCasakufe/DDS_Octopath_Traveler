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

        context.Traveler.RecoverHp(context.Traveler.MaxHp / HealingDivisor);
    }
}

namespace Octopath_Traveler_Models.Battle;

internal sealed class SecondWindPassiveSkillHandler
    : TravelerPassiveSkillHandler, RoundEndPassiveSkillHandler
{
    private const int RecoveryDivisor = 20;

    public SecondWindPassiveSkillHandler(TravelerCombatUnit traveler)
        : base(traveler)
    {
    }

    public void Handle(RoundEndPassiveRecoveryContext context)
    {
        if (!context.Traveler.IsAlive)
            return;

        context.Traveler.RecoverSp(context.Traveler.MaxSp / RecoveryDivisor);
    }
}

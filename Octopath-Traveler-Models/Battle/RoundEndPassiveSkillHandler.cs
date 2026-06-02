namespace Octopath_Traveler_Models.Battle;

internal interface RoundEndPassiveSkillHandler
{
    TravelerCombatUnit Traveler { get; }

    void Handle(RoundEndPassiveRecoveryContext context);
}

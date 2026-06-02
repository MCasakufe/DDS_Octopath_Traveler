namespace Octopath_Traveler_Models.Battle;

internal sealed class AllDefeatedTravelersTravelerSkillTargetSelector : EligibleTravelersTravelerSkillTargetSelector
{
    protected override bool IsEligibleTraveler(TravelerCombatUnit traveler)
        => !traveler.IsAlive;
}

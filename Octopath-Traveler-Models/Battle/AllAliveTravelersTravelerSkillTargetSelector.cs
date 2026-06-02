namespace Octopath_Traveler_Models.Battle;

internal sealed class AllAliveTravelersTravelerSkillTargetSelector : EligibleTravelersTravelerSkillTargetSelector
{
    protected override bool IsEligibleTraveler(TravelerCombatUnit traveler)
        => traveler.IsAlive;
}

namespace Octopath_Traveler_Models.Battle;

internal interface ExtraTurnPassiveSkillHandler
{
    TravelerCombatUnit Traveler { get; }

    bool CanGrantExtraTurn(PassiveExtraTurnEligibilityContext context);
}

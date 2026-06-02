namespace Octopath_Traveler_Models.Battle;

internal abstract class TravelerPassiveSkillHandler
{
    protected TravelerPassiveSkillHandler(TravelerCombatUnit traveler)
    {
        Traveler = traveler;
    }

    public TravelerCombatUnit Traveler { get; }
}

namespace Octopath_Traveler_Models.Battle;

internal sealed record TravelerDamageApplication(
    TravelerCombatUnit Target,
    int Damage);

internal sealed class TravelerDamageApplier
{
    public void ApplyDamage(TravelerDamageApplication damageApplication)
        => damageApplication.Target.ReceiveDamage(damageApplication.Damage);
}

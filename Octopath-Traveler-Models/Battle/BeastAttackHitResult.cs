namespace Octopath_Traveler_Models.Battle;

internal sealed record BeastAttackHitResult(
    int Damage,
    bool WasDefended,
    bool RevivedByEncore);

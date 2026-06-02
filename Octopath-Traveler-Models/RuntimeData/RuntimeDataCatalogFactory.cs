namespace Octopath_Traveler_Models.RuntimeData;

internal sealed class RuntimeDataCatalogFactory
{
    public RuntimeDataCatalog Create(RuntimeDataDefinitions definitions)
    {
        IReadOnlySet<string> activeSkillNames = new HashSet<string>(
            definitions.ActiveSkillsByName.Keys,
            StringComparer.Ordinal);
        IReadOnlySet<string> passiveSkillNames = new HashSet<string>(
            definitions.PassiveSkillsByName.Keys,
            StringComparer.Ordinal);
        IReadOnlySet<string> beastSkillNames = new HashSet<string>(
            definitions.BeastSkillsByName.Keys,
            StringComparer.Ordinal);

        return new RuntimeDataCatalog(
            definitions.TravelersByName,
            definitions.BeastsByName,
            definitions.ActiveSkillsByName,
            definitions.BeastSkillsByName,
            definitions.PassiveSkillsByName,
            activeSkillNames,
            passiveSkillNames,
            beastSkillNames);
    }
}

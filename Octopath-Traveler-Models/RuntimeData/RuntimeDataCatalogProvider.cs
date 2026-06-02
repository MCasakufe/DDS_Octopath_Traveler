namespace Octopath_Traveler_Models.RuntimeData;

public sealed class RuntimeDataCatalogProvider
{
    private readonly RuntimeDataFileReader _fileReader;
    private readonly RuntimeDataCatalogFactory _catalogFactory = new();
    private readonly TravelerDefinitionParser _travelerDefinitionParser = new();
    private readonly BeastDefinitionParser _beastDefinitionParser = new();
    private readonly SkillDefinitionParser _skillDefinitionParser = new();
    private readonly BeastSkillDefinitionParser _beastSkillDefinitionParser = new();
    private readonly PassiveSkillDefinitionParser _passiveSkillDefinitionParser = new();

    public RuntimeDataCatalogProvider(string teamsDirectoryPath)
    {
        string runtimeDataDirectoryPath = Path.GetDirectoryName(teamsDirectoryPath) ?? string.Empty;
        _fileReader = new RuntimeDataFileReader(runtimeDataDirectoryPath);
    }

    public RuntimeDataCatalog Load()
    {
        RuntimeDataDefinitions definitions = new(
            LoadDefinitions(RuntimeDataFileNames.Characters, _travelerDefinitionParser.Parse),
            LoadDefinitions(RuntimeDataFileNames.Enemies, _beastDefinitionParser.Parse),
            LoadDefinitions(RuntimeDataFileNames.ActiveSkills, _skillDefinitionParser.Parse),
            LoadDefinitions(RuntimeDataFileNames.BeastSkills, _beastSkillDefinitionParser.Parse),
            LoadDefinitions(RuntimeDataFileNames.PassiveSkills, _passiveSkillDefinitionParser.Parse));

        return _catalogFactory.Create(definitions);
    }

    private Dictionary<string, TDefinition> LoadDefinitions<TDefinition>(
        string fileName,
        Func<string, string, Dictionary<string, TDefinition>> parseFile)
    {
        string jsonContent = _fileReader.Read(fileName);
        return parseFile(fileName, jsonContent);
    }
}

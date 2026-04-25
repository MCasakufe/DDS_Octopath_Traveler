namespace Octopath_Traveler_View;

class Script
{
    private const string InputKeyword = "INPUT: ";
    private string _scriptText = "";
    
    public void AddInput(string inputFromUser)
        => AppendToScript($"{InputKeyword}{inputFromUser}\n");
    
    public void AppendToScript(string message)
        => _scriptText += message;

    public string GetScriptText()
        => _scriptText;
    
    public void ExportScript(string outputPath) 
        => File.WriteAllText(outputPath, _scriptText);
}

namespace Octopath_Traveler_View;

public abstract class AbstractView
{
    private readonly Script _script = new();
    
    public void WriteLine(object text)
        => WriteOutput($"{text}\n");

    protected virtual void WriteOutput(object text)
        => _script.AppendToScript(text.ToString());

    public string ReadLine()
    {
        string nextInput = GetNextInput();
        _script.AddInput(nextInput);
        return nextInput;
    }
    
    protected abstract string GetNextInput();
    
    public void ExportScript(string path)
        => _script.ExportScript(path);

    public string[] GetScript()
        => _script.GetScriptText().Split("\n");
}

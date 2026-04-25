namespace Octopath_Traveler_View;

public class TestingView : AbstractView
{
    private const string InputKeyword = "INPUT: ";
    private readonly string[] _expectedScript;
    private readonly Queue<string> _inputsFromUser = new();
    
    public TestingView(string pathTestScript)
    {
        _expectedScript = File.ReadAllLines(pathTestScript);
        LoadUserInputQueueFromScript();
    }
    
    private void LoadUserInputQueueFromScript()
    {
        foreach (string scriptLine in _expectedScript)
            if (IsUserInputLine(scriptLine))
                _inputsFromUser.Enqueue(scriptLine.Replace(InputKeyword, ""));
    }
    
    private bool IsUserInputLine(string scriptLine)
        => scriptLine.StartsWith(InputKeyword);

    protected override void RenderOutput(string text)
    {
    }

    protected override string GetNextInput()
    {
        if (_inputsFromUser.Any())
            return _inputsFromUser.Dequeue();
        throw new ApplicationException("Tu programa pidió un input pero no hay más inputs del usuario en este test case!");
    }
}

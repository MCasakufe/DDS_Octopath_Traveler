namespace Octopath_Traveler_View.Main;

public sealed class MainConsoleView
{
    private const string InvalidTeamFileMessage = "Archivo de equipos no v\u00E1lido";

    private readonly View _view;

    public MainConsoleView(View view)
    {
        _view = view;
    }

    public void WriteInvalidTeamFileMessage()
        => _view.WriteLine(InvalidTeamFileMessage);
}

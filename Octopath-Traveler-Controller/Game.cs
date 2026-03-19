using Octopath_Traveler_View;

namespace Octopath_Traveler;

public class Game
{
    private View _view;
    public Game(View view, string teamsFolder)
    {
        _view = view;
    }

    public void Play()
    {
        _view.WriteLine("Test");
        throw new NotImplementedException();
    }
}
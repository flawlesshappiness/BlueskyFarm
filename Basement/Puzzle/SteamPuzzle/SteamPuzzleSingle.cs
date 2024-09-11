public partial class SteamPuzzleSingle : SteamPuzzle
{
    [NodeType]
    public InteractableLever Lever;

    public override void _Ready()
    {
        base._Ready();

        Lever.OnStateChanged += StateChanged;
    }

    private void StateChanged(int state)
    {
        if (state == 0)
        {
            StartSteam();
        }
        else
        {
            StopSteam();
        }
    }
}

using Godot;
using GodotSteam;

public partial class SteamTest : Node
{
    public override void _Ready()
    {
        return;
        var isSteamRunning = Steam.IsSteamRunning();
        if (!isSteamRunning)
        {
            GD.Print("Steam is not running.");
            return;
        }

        var steamId = Steam.GetSteamID();
        var name = Steam.GetFriendPersonaName(steamId);

        GD.Print("Your Steam Name: " + name);
    }
}

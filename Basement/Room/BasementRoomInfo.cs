using Godot;

[GlobalClass]
public partial class BasementRoomInfo : Resource
{
    [Export]
    public string Area;

    [Export]
    public string ConnectedArea;

    [Export]
    public string AmbienceArea;

    [Export]
    public PackedScene Scene;

    [Export]
    public bool IsStartRoom;

    [Export]
    public bool IsConnectedToAllFromSameArea;

    [Export(PropertyHint.MultilineText)]
    public string RoomLayout;

    [Export]
    public bool HasUnevenGround;

    [Export]
    public bool Disabled;
}

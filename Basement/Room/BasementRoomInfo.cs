using Godot;

[GlobalClass]
public partial class BasementRoomInfo : Resource
{
    [Export]
    public string Area;

    [Export]
    public string ConnectedArea;

    [Export(PropertyHint.File)]
    public string Path;

    [Export]
    public bool IsStartRoom;

    [Export]
    public bool IsConnectedToAllFromSameArea;

    [Export(PropertyHint.MultilineText)]
    public string RoomLayout;

    [Export]
    public bool Disabled;
}

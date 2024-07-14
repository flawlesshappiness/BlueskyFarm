using Godot;

[GlobalClass]
public partial class BasementRoomInfo : Resource
{
    [Export(PropertyHint.File)]
    public string Path;

    [Export]
    public bool IsStartRoom;

    [Export(PropertyHint.MultilineText)]
    public string RoomLayout;

    [Export]
    public bool Disabled;
}

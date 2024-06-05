using Godot;

[GlobalClass]
public partial class BasementRoomInfo : Resource
{
    [Export(PropertyHint.File)]
    public string Path;

    [Export(PropertyHint.File)]
    public bool IsStartRoom;

    [Export(PropertyHint.MultilineText)]
    public string RoomLayout;
}

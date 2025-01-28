using Godot;
using Godot.Collections;

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
    public Array<float> ValidGroundHeights;

    [Export]
    public bool Disabled;
}

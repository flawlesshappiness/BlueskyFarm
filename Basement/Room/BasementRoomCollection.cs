using Godot;

[GlobalClass]
public partial class BasementRoomCollection : ResourceCollection<BasementRoomInfo>
{
    [Export(PropertyHint.File)]
    public string MaterialProcessorOilRoom;
}

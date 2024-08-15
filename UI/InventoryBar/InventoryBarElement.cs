using Godot;

public partial class InventoryBarElement : ControlScript
{
    [NodeName]
    public TextureRect Icon;

    public void Clear()
    {
        Icon.Texture = null;
    }
}

using Godot;

public partial class InventoryBarElement : ControlScript
{
    [NodeName]
    public TextureRect Icon;

    [NodeName]
    public TextureRect Selected;

    public void Clear()
    {
        Icon.Texture = null;
    }

    public void Select()
    {
        Selected.Visible = true;
    }

    public void Deselect()
    {
        Selected.Visible = false;
    }
}

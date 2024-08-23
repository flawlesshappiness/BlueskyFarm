using Godot;

public partial class InventoryBarElement : ControlScript
{
    [NodeName]
    public TextureRect Selected;

    [NodeName]
    public WorldObject WorldObject;

    public void Clear()
    {
        WorldObject.Clear();
    }

    public void Select()
    {
        Selected.Visible = true;
        WorldObject.StartAnimateSpin();
    }

    public void Deselect()
    {
        Selected.Visible = false;
        WorldObject.StopAnimateSpin();
    }
}

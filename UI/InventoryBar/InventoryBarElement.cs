using Godot;

public partial class InventoryBarElement : ControlScript
{
    [Export]
    public Label UseLabel;

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
        Selected.Show();
        WorldObject.StartAnimateSpin();
    }

    public void Deselect()
    {
        Selected.Hide();
        UseLabel.Hide();
        WorldObject.StopAnimateSpin();
    }
}

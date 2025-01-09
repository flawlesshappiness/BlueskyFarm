using Godot;

public partial class InventoryBarElement : ControlScript
{
    [Export]
    public Label UseLabel;

    [Export]
    public TextureRect SelectedTexture;

    [Export]
    public WorldObject WorldObject;

    public bool Selected { get; private set; }

    public void Clear()
    {
        WorldObject.Clear();
    }

    public void Select()
    {
        Selected = true;
        SelectedTexture.Show();
        WorldObject.StartAnimateSpin();
    }

    public void Deselect()
    {
        Selected = false;
        SelectedTexture.Hide();
        UseLabel.Hide();
        WorldObject.StopAnimateSpin();
    }
}

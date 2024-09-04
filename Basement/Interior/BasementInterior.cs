using Godot;

public partial class BasementInterior : Node3DScript
{
    [Export]
    public bool ExcludedFromGroup;

    public void Activate()
    {
        this.SetEnabled(true);
    }

    public void Deactivate()
    {
        this.SetEnabled(false);
    }
}

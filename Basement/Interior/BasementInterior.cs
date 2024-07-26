using Godot;

public partial class BasementInterior : Node3DScript
{
    [Export]
    public bool ExcludedFromGroup;

    public void Activate()
    {
        var parent = GetParent() as BasementInteriorGroup;
        parent.SetInterior(this);
    }
}

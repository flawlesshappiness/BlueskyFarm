using Godot;

public partial class FrogBlueprintCrafting : Node3DScript
{
    [NodeName]
    public Area3D ItemArea;

    public override void _Ready()
    {
        base._Ready();
    }

    private void BodyEntered(GodotObject go)
    {
        var node = go as Node3D;
        if (node is Item item && item != null)
        {

        }
    }
}

using Godot;

public partial class Node3DScript : Node3D
{
    public override void _Ready()
    {
        NodeScript.FindNodesFromAttribute(this, GetType());

        base._Ready();
    }
}

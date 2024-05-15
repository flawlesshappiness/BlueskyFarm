using Godot;

public partial class ControlScript : Control
{
    public override void _Ready()
    {
        NodeScript.FindNodesFromAttribute(this, GetType());

        base._Ready();
    }
}

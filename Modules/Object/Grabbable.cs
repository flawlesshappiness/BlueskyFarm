using Godot;

public partial class Grabbable : RigidBody3D, IGrabbable, IInteractable
{
    public Node3D Node => this;

    public override void _Ready()
    {
        base._Ready();
        NodeScript.FindNodesFromAttribute(this, GetType());
    }

    public void Grabbed()
    {
        Freeze = true;
    }

    public void Released()
    {
        Freeze = false;
        LinearVelocity = Vector3.Zero;
    }

    public void SetPosition(Vector3 position)
    {
        Node.GlobalPosition = position;
    }
}

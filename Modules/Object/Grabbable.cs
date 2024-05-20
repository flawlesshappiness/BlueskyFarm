using Godot;

public partial class Grabbable : RigidBody3D, IGrabbable, IInteractable
{
    [Export]
    public float MaxThrowVelocity;

    public Node3D Node => this;
    public bool IsGrabbed => is_grabbed;

    private bool is_grabbed;
    private Vector3 target_position;
    private Vector3 target_rotation;

    public override void _Ready()
    {
        base._Ready();
        NodeScript.FindNodesFromAttribute(this, GetType());
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        PhysicsProcess_MoveWhenGrabbed();
        PhysicsProcess_RotateWhenGrabbed();
    }

    private void PhysicsProcess_MoveWhenGrabbed()
    {
        if (!IsGrabbed) return;

        var direction = GlobalPosition.DirectionTo(target_position);
        var distance = GlobalPosition.DistanceTo(target_position);
        var velocity = direction * distance * 10;
        LinearVelocity = velocity;
    }

    private void PhysicsProcess_RotateWhenGrabbed()
    {
        if (!IsGrabbed) return;

        GlobalRotation = target_rotation;
        AngularVelocity = Vector3.Zero;
    }

    public void Grabbed()
    {
        is_grabbed = true;
        GravityScale = 0;
    }

    public void Released()
    {
        is_grabbed = false;
        GravityScale = 1;

        if (MaxThrowVelocity > 0 && LinearVelocity.Length() > MaxThrowVelocity)
        {
            LinearVelocity = LinearVelocity.Normalized() * MaxThrowVelocity;
        }
    }

    public void SetPosition(Vector3 position)
    {
        target_position = position;
    }

    public void SetRotation(Vector3 rotation)
    {
        target_rotation = rotation;
    }
}

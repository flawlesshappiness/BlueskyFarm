using Godot;
using System;

public partial class Grabbable : RigidBody3D, IGrabbable, IInteractable
{
    [Export]
    public float MaxThrowVelocity;

    public Node3D Node => this;
    public bool IsGrabbed => is_grabbed;

    private bool is_grabbed;
    private Vector3 target_position;
    private Vector3 previous_position;
    private Vector3 intended_velocity;

    public override void _Ready()
    {
        base._Ready();
        NodeScript.FindNodesFromAttribute(this, GetType());
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        var f_delta = Convert.ToSingle(delta);
        PhysicsProcess_MoveWhenGrabbed(f_delta);
        PhysicsProcess_CalculateVelocity(f_delta);
    }

    private void PhysicsProcess_MoveWhenGrabbed(float delta)
    {
        if (!IsGrabbed) return;

        var direction = GlobalPosition.DirectionTo(target_position);
        var distance = GlobalPosition.DistanceTo(target_position);
        var velocity = direction * distance * 10;
        LinearVelocity = velocity;
    }

    private void PhysicsProcess_CalculateVelocity(float delta)
    {
        if (!IsGrabbed) return;

        var p = GlobalPosition;
        var dir = p - previous_position;
        var d = previous_position.DistanceTo(p);
        var v = d / delta;

        intended_velocity = dir.Normalized() * v;
        previous_position = p;
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
        //LinearVelocity = intended_velocity;

        if (MaxThrowVelocity > 0 && LinearVelocity.Length() > MaxThrowVelocity)
        {
            LinearVelocity = LinearVelocity.Normalized() * MaxThrowVelocity;
        }
    }

    public void SetPosition(Vector3 position)
    {
        target_position = position;
    }
}

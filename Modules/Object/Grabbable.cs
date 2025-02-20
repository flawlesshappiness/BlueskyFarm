using Godot;
using System;

public partial class Grabbable : InteractableRigidBody3D
{
    [Export]
    public float MaxThrowVelocity = 12;

    public bool IsGrabbable { get; private set; }
    public bool IsGrabbed { get; private set; }
    public Vector3 TargetPosition { get; set; }
    public Vector3 TargetRotation { get; set; }

    public event Action OnGrabbed;
    public event Action OnReleased;

    public override void _Ready()
    {
        base._Ready();
        CanSleep = false;
        IsGrabbable = true;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        PhysicsProcess_MoveWhenGrabbed();
        PhysicsProcess_RotateWhenGrabbed();
    }

    private void PhysicsProcess_MoveWhenGrabbed()
    {
        if (!IsGrabbed || !IsGrabbable) return;

        var direction = GlobalPosition.DirectionTo(TargetPosition);
        var distance = GlobalPosition.DistanceTo(TargetPosition);
        var velocity = direction * distance * 10;
        LinearVelocity = velocity;
    }

    private void PhysicsProcess_RotateWhenGrabbed()
    {
        if (!IsGrabbed || !IsGrabbable) return;

        GlobalRotation = TargetRotation;
        AngularVelocity = Vector3.Zero;
    }

    public void Grabbed()
    {
        IsGrabbed = true;
        GravityScale = 0;

        OnGrabbed?.Invoke();
    }

    public void Released()
    {
        IsGrabbed = false;
        GravityScale = 1;

        if (MaxThrowVelocity > 0 && LinearVelocity.Length() > MaxThrowVelocity)
        {
            LinearVelocity = LinearVelocity.Normalized() * MaxThrowVelocity;
        }

        OnReleased?.Invoke();
    }

    public void SetGrabbable(bool grabbable)
    {
        IsGrabbable = grabbable;
    }
}
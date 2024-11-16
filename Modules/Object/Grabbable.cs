using Godot;
using System;

public partial class Grabbable : Interactable
{
    [Export]
    public float MaxThrowVelocity = 12;

    public RigidBody3D RigidBody => _rigidBody ??= Body as RigidBody3D;
    private RigidBody3D _rigidBody;

    public bool IsGrabbable { get; private set; }
    public bool IsGrabbed { get; private set; }
    public Vector3 TargetPosition { get; set; }
    public Vector3 TargetRotation { get; set; }

    public event Action OnGrabbed;
    public event Action OnReleased;

    public override void _Ready()
    {
        base._Ready();
        RigidBody.CanSleep = false;
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
        if (!IsGrabbed) return;

        var direction = RigidBody.GlobalPosition.DirectionTo(TargetPosition);
        var distance = RigidBody.GlobalPosition.DistanceTo(TargetPosition);
        var velocity = direction * distance * 10;
        RigidBody.LinearVelocity = velocity;
    }

    private void PhysicsProcess_RotateWhenGrabbed()
    {
        if (!IsGrabbed) return;

        RigidBody.GlobalRotation = TargetRotation;
        RigidBody.AngularVelocity = Vector3.Zero;
    }

    public void Grabbed()
    {
        IsGrabbed = true;
        RigidBody.GravityScale = 0;

        OnGrabbed?.Invoke();
    }

    public void Released()
    {
        IsGrabbed = false;
        RigidBody.GravityScale = 1;

        if (MaxThrowVelocity > 0 && RigidBody.LinearVelocity.Length() > MaxThrowVelocity)
        {
            RigidBody.LinearVelocity = RigidBody.LinearVelocity.Normalized() * MaxThrowVelocity;
        }

        OnReleased?.Invoke();
    }

    public void SetGrabbable(bool grabbable)
    {
        IsGrabbable = grabbable;
    }
}
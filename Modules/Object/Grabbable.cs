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

    public event Action OnGrabbed;
    public event Action OnReleased;

    private Vector3 target_position;
    private Vector3 target_rotation;

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

        var direction = RigidBody.GlobalPosition.DirectionTo(target_position);
        var distance = RigidBody.GlobalPosition.DistanceTo(target_position);
        var velocity = direction * distance * 10;
        RigidBody.LinearVelocity = velocity;
    }

    private void PhysicsProcess_RotateWhenGrabbed()
    {
        if (!IsGrabbed) return;

        RigidBody.GlobalRotation = target_rotation;
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

    public void ReleaseIfGrabbed()
    {
        var player = Player.Instance; // TODO: Should not reference player
        if (player.Grab.GetCurrentTarget() == this)
        {
            player.Grab.Release();
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

    public void SetGrabbable(bool grabbable)
    {
        IsGrabbable = grabbable;

        if (!IsGrabbable)
        {
            ReleaseIfGrabbed();
        }
    }
}
using Godot;
using System;

public partial class Grabbable : RigidBody3D, IGrabbable, IInteractable
{
    [Export]
    public float MaxThrowVelocity = 12;

    [Export]
    public InteractCollisionMode InitialCollisionMode = InteractCollisionMode.All;

    public Node3D Node => this;
    public bool IsGrabbed => is_grabbed;

    public event Action OnGrabbed;
    public event Action OnReleased;

    private bool is_grabbed;
    private Vector3 target_position;
    private Vector3 target_rotation;

    public override void _Ready()
    {
        base._Ready();
        NodeScript.FindNodesFromAttribute(this, GetType());

        SetCollisionMode(InitialCollisionMode);
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

        OnGrabbed?.Invoke();
    }

    public void Released()
    {
        is_grabbed = false;
        GravityScale = 1;

        if (MaxThrowVelocity > 0 && LinearVelocity.Length() > MaxThrowVelocity)
        {
            LinearVelocity = LinearVelocity.Normalized() * MaxThrowVelocity;
        }

        OnReleased?.Invoke();
    }

    public void SetPosition(Vector3 position)
    {
        target_position = position;
    }

    public void SetRotation(Vector3 rotation)
    {
        target_rotation = rotation;
    }

    public void SetCollisionMode(InteractCollisionMode mode)
    {
        CollisionLayer = mode switch
        {
            InteractCollisionMode.All => CollisionMaskHelper.Create(1, 3),
            InteractCollisionMode.Collision => CollisionMaskHelper.Create(1),
            InteractCollisionMode.Interact => CollisionMaskHelper.Create(3),
            _ => 0
        };

        CollisionMask = mode switch
        {
            InteractCollisionMode.All => CollisionMaskHelper.Create(1),
            InteractCollisionMode.Collision => CollisionMaskHelper.Create(1),
            InteractCollisionMode.Interact => CollisionMaskHelper.Create(1),
            _ => 0
        };
    }
}

public enum InteractCollisionMode
{
    None, All, Collision, Interact
}

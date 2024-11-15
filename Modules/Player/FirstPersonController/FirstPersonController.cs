using Godot;
using System;

public partial class FirstPersonController : CharacterBody3D
{
    [NodeName]
    public CollisionShape3D PlayerCollisionShape;

    [NodeName]
    public Node3D NeckHorizontal;

    [NodeName]
    public Node3D NeckVertical;

    protected CapsuleShape3D Capsule => PlayerCollisionShape.Shape as CapsuleShape3D;
    public Vector3 DesiredMoveVelocity { get; private set; }
    public Vector3 DesiredJumpVelocity { get; private set; }

    private float _gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();
    private bool _moving;
    private bool _jumping;
    private Node3D _look_at_target;
    private float _look_at_speed;

    public Action OnJump, OnLand;
    public Action OnMoveStart, OnMoveStop;

    public override void _Ready()
    {
        base._Ready();
        NodeScript.FindNodesFromAttribute(this, GetType());
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        Process_RotateView();
    }

    public override void _PhysicsProcess(double delta)
    {
        PhysicsProcess_Move(delta);
    }

    protected void Look(InputEventMouseMotion motion, float speed)
    {
        Look(motion.Relative * speed);
    }

    protected void Look(Vector2 direction)
    {
        NeckHorizontal.RotateY(-direction.X);
        NeckVertical.RotateX(-direction.Y);

        var x = Mathf.Clamp(NeckVertical.Rotation.X, Mathf.DegToRad(-70), Mathf.DegToRad(70));
        NeckVertical.Rotation = Rotation with { X = x };
    }

    private void Process_RotateView()
    {
        if (!IsInstanceValid(_look_at_target)) return;

        var target_position = _look_at_target.GlobalPosition;

        var hor_angle = GetHorizontalAngleToPoint(target_position);
        var hor_angle_lerp = Mathf.Lerp(0, hor_angle, _look_at_speed * GameTime.DeltaTime);
        if (Mathf.Abs(hor_angle) > 1f)
        {
            NeckHorizontal.RotateY(hor_angle_lerp);
        }

        var ver_angle = GetVerticalAngleToPoint(target_position);
        var ver_angle_lerp = Mathf.Lerp(0, ver_angle, _look_at_speed * GameTime.DeltaTime);
        if (Mathf.Abs(ver_angle) > 1f)
        {
            NeckVertical.RotateX(ver_angle_lerp);
        }
    }

    public float GetHorizontalAngleToPoint(Vector3 point)
    {
        var dir = NeckHorizontal.GlobalPosition.Set(y: point.Y) - point;
        var rad = NeckHorizontal.GlobalBasis.Z.SignedAngleTo(dir, Vector3.Up);
        var deg = Mathf.RadToDeg(rad);
        return deg;
    }

    public float GetVerticalAngleToPoint(Vector3 point)
    {
        var dist = NeckVertical.GlobalPosition.Set(y: point.Y).DistanceTo(point);
        var new_point = NeckVertical.GlobalPosition.Set(y: point.Y) - NeckHorizontal.GlobalBasis.Z.Normalized() * dist;
        var dir_to_new_point = new_point - NeckVertical.GlobalPosition;
        var dir_forward = -NeckVertical.GlobalBasis.Z;
        var dir_right = NeckVertical.GlobalBasis.X;
        var rad = dir_forward.SignedAngleTo(dir_to_new_point, dir_right);
        var deg = Mathf.RadToDeg(rad);
        return deg;
    }

    private void PhysicsProcess_Move(double delta)
    {
        Vector3 velocity = Velocity;
        var grounded = IsOnFloor();

        if (grounded)
        {
            if (_jumping)
            {
                _jumping = false;
                OnLand?.Invoke();
            }

            if (DesiredMoveVelocity != Vector3.Zero)
            {
                if (!_moving)
                {
                    _moving = true;
                    OnMoveStart?.Invoke();
                }

                velocity.X = DesiredMoveVelocity.X;
                velocity.Z = DesiredMoveVelocity.Z;
            }
            else
            {
                if (_moving)
                {
                    _moving = false;
                    OnMoveStop?.Invoke();
                }

                var decel = 15 * (float)delta;
                velocity.X = Mathf.MoveToward(Velocity.X, 0, decel);
                velocity.Z = Mathf.MoveToward(Velocity.Z, 0, decel);
            }

            if (DesiredJumpVelocity != Vector3.Zero)
            {
                velocity += DesiredJumpVelocity;
                _jumping = true;
                OnJump?.Invoke();
            }
        }
        else
        {
            velocity.Y -= _gravity * (float)delta;
        }

        Velocity = velocity;
        MoveAndSlide();
    }

    protected void Move(Vector2 input, float speed)
    {
        if (input.Length() > 0)
        {
            Vector3 direction = (NeckHorizontal.Basis * new Vector3(input.X, 0, input.Y)).Normalized();
            Move(direction * speed);
        }
        else
        {
            Stop();
        }
    }

    protected void Move(Vector3 velocity)
    {
        DesiredMoveVelocity = velocity;
    }

    protected void Stop()
    {
        DesiredMoveVelocity = Vector3.Zero;
    }

    protected void Jump(Vector3 velocity)
    {
        DesiredJumpVelocity = velocity;
    }

    public void SetLookRotation(Node3D node)
    {
        NeckVertical.Rotation = new Vector3(node.GlobalRotation.X, 0, 0);
        NeckHorizontal.Rotation = new Vector3(0, node.GlobalRotation.Y, 0);
    }

    public void StartLookingAt(Node3D target, float speed)
    {
        _look_at_target = target;
        _look_at_speed = speed;
    }

    public void StopLookingAt(Node3D target)
    {
        if (_look_at_target == target)
        {
            StopLookingAt();
        }
    }

    public void StopLookingAt()
    {
        _look_at_target = null;
    }
}

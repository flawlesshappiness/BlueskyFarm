using Godot;
using System;

public partial class FirstPersonController : CharacterBody3D
{
    public static FirstPersonController Instance { get; private set; }

    [Export]
    public float WalkSpeed = 2.5f;

    [Export]
    public float RunSpeed = 5.0f;

    [Export]
    public float JumpUpSpeed = 3f;

    [Export]
    public float JumpHorizontalSpeed;

    // Get the gravity from the project settings to be synced with RigidBody nodes.
    public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

    [NodeName]
    public Node3D NeckHorizontal;

    [NodeName]
    public Node3D NeckVertical;

    [NodeName]
    public Node3D Mid;

    [NodeName]
    public Camera3D Camera;

    [NodeType]
    public IPlayerInteract Interact;

    [NodeType]
    public IPlayerGrab Grab;

    [NodeType]
    public FirstPersonStep Step;

    [NodeType]
    public NavigationAgent3D Agent;

    public float MoveSpeedMultiplier { get; set; } = 1f;
    public float LookSpeedMultiplier { get; set; } = 1f;
    public bool IsRunning => PlayerInput.Run.Held;
    public float DesiredMoveSpeed => IsRunning ? RunSpeed : WalkSpeed;

    public MultiLock InteractLock = new MultiLock();
    public MultiLock MovementLock = new MultiLock();
    public MultiLock LookLock = new MultiLock();

    private bool _jumping;
    private static bool _debug_actions_registered;
    private Node3D _look_at_target;
    private float _look_at_speed;

    public Action OnJump, OnLand;

    public override void _Ready()
    {
        base._Ready();
        Instance = this;
        NodeScript.FindNodesFromAttribute(this, GetType());

        LoadData();

        RegisterDebugActions();
    }

    private void RegisterDebugActions()
    {
        if (_debug_actions_registered) return;
        _debug_actions_registered = true;

        var category = "Player";

        Debug.RegisterAction(new DebugAction
        {
            Category = category,
            Text = "Unstuck",
            Action = Unstuck
        });
    }

    private void Unstuck(DebugView view)
    {
        var player = FirstPersonController.Instance;
        var agent = player.Agent;
        var target_position = player.GlobalPosition - player.Camera.GlobalBasis.Z * 2f;
        var nav_position = NavigationServer3D.MapGetClosestPoint(agent.GetNavigationMap(), target_position) - new Vector3(0, agent.PathHeightOffset, 0);
        player.GlobalPosition = nav_position;
        view.SetVisible(false);
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        Input_RotateView(@event);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        Process_Interact();
        Process_Cursor();
        Process_RotateView();
    }

    private void Input_RotateView(InputEvent e)
    {
        if (LookLock.IsLocked) return;
        if (MovementLock.IsLocked) return;
        if (Input.MouseMode != Input.MouseModeEnum.Captured) return;
        if (e is not InputEventMouseMotion) return;
        if (Grab?.IsRotating ?? false) return;

        var factor = 0.001f;
        var motion = e as InputEventMouseMotion;
        NeckHorizontal.RotateY(-motion.Relative.X * factor * LookSpeedMultiplier);
        NeckVertical.RotateX(-motion.Relative.Y * factor * LookSpeedMultiplier);

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
        if (Mathf.Abs(ver_angle) > 5f)
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

    public override void _PhysicsProcess(double delta)
    {
        Vector3 velocity = Velocity;
        var grounded = IsOnFloor();

        // Add the gravity.
        if (!grounded)
            velocity.Y -= gravity * (float)delta;

        // Get the input direction and handle the movement/deceleration.
        Vector2 inputDir = Input.GetVector(
            PlayerInput.Left.Name,
            PlayerInput.Right.Name,
            PlayerInput.Forward.Name,
            PlayerInput.Back.Name);

        Vector3 direction = (NeckHorizontal.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
        if (MovementLock.IsLocked) direction *= 0;

        if (grounded)
        {
            if (_jumping)
            {
                _jumping = false;
                OnLand?.Invoke();
            }

            var has_direction = direction != Vector3.Zero;
            if (has_direction)
            {
                velocity.X = direction.X * DesiredMoveSpeed * MoveSpeedMultiplier;
                velocity.Z = direction.Z * DesiredMoveSpeed * MoveSpeedMultiplier;
            }
            else
            {
                velocity.X = Mathf.MoveToward(Velocity.X, 0, DesiredMoveSpeed * MoveSpeedMultiplier);
                velocity.Z = Mathf.MoveToward(Velocity.Z, 0, DesiredMoveSpeed * MoveSpeedMultiplier);
            }

            // Handle Jump
            if (JumpUpSpeed > 0 && PlayerInput.Jump.Pressed)
            {
                var dir = new Vector3(direction.X, 0, direction.Z).Normalized() * JumpHorizontalSpeed;
                var jump_vel = new Vector3(dir.X, JumpUpSpeed, dir.Z);
                velocity += jump_vel;

                _jumping = true;

                OnJump?.Invoke();
            }
        }

        Velocity = velocity;
        MoveAndSlide();
    }

    private void Process_Cursor()
    {
        var target = Interact?.CurrentInteractable;

        if (InteractLock.IsLocked || !IsInstanceValid(target))
        {
            Cursor.Hide();
        }
        else if (Grab?.TryHandleCursor(target) ?? false)
        {
            // Handled by grab
        }
        else if (TryHandleCursor_Touch(target))
        {
            // Handled by touch
        }
        else
        {
            Cursor.Hide();
        }
    }

    private bool TryHandleCursor_Touch(Interactable interactable)
    {
        var touchable = interactable as Touchable;
        if (!IsInstanceValid(touchable)) return false;

        if (touchable.HandleCursor())
        {
            // Handled by touchable
            return true;
        }
        else
        {
            Cursor.Show(new CursorSettings
            {
                Name = "Touch",
                OverrideTexture = interactable.OverrideCursorTexture,
                Text = interactable.InteractableText,
                Position = interactable.Body.GlobalPosition
            });
        }

        return true;
    }

    private void Process_Interact()
    {
        if (InteractLock.IsLocked) return;

        if (PlayerInput.Interact.Pressed)
        {
            var interactable = Interact.CurrentInteractable;
            if (TryGrab(interactable)) return;
            if (TryTouch(interactable)) return;
        }
        else if (PlayerInput.Interact.Released)
        {
            Grab?.Release();
        }
    }

    private bool TryGrab(Interactable interactable)
    {
        if (Grab == null) return false;
        if (interactable == null) return false;

        var grabbable = interactable as Grabbable;
        if (grabbable == null) return false;

        Grab.Grab(grabbable);
        return true;
    }

    private bool TryTouch(Interactable interactable)
    {
        if (interactable == null) return false;

        var touchable = interactable as Touchable;
        if (touchable == null) return false;

        touchable.Touch();
        return true;
    }

    public void SetLookRotation(Node3D node)
    {
        NeckVertical.Rotation = new Vector3(node.GlobalRotation.X, 0, 0);
        NeckHorizontal.Rotation = new Vector3(0, node.GlobalRotation.Y, 0);
    }

    public void UpdateData()
    {
        Data.Game.PlayerPositionX = GlobalPosition.X;
        Data.Game.PlayerPositionY = GlobalPosition.Y;
        Data.Game.PlayerPositionZ = GlobalPosition.Z;
        Data.Game.PlayerNeckVerticalRotation = NeckVertical.Rotation.X;
        Data.Game.PlayerNeckHorizontalRotation = NeckHorizontal.Rotation.Y;
    }

    public void LoadData()
    {
        GlobalPosition = new Vector3(Data.Game.PlayerPositionX, Data.Game.PlayerPositionY, Data.Game.PlayerPositionZ);
        NeckVertical.Rotation = new Vector3(Data.Game.PlayerNeckVerticalRotation, 0, 0);
        NeckHorizontal.Rotation = new Vector3(0, Data.Game.PlayerNeckHorizontalRotation, 0);
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

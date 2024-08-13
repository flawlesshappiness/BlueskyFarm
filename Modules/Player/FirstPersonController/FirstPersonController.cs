using Godot;
using System;
using System.Collections;

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

    [NodeName(nameof(Neck))]
    public Node3D Neck;

    [NodeName(nameof(Mid))]
    public Node3D Mid;

    [NodeName(nameof(Camera))]
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

        view.SetVisible(false);
        Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            agent.TargetPosition = player.GlobalPosition;
            yield return null;
            player.GlobalPosition = agent.GetNextPathPosition();
        }
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
        Neck.RotateY(-motion.Relative.X * factor * LookSpeedMultiplier);
        Camera.RotateX(-motion.Relative.Y * factor * LookSpeedMultiplier);

        var x = Mathf.Clamp(Camera.Rotation.X, Mathf.DegToRad(-70), Mathf.DegToRad(70));
        Camera.Rotation = Rotation with { X = x };
    }

    private void Process_RotateView()
    {
        if (!IsInstanceValid(_look_at_target)) return;

        var target_position = _look_at_target.GlobalPosition;
        var target_distance = GlobalPosition.DistanceTo(target_position);

        var y_position = target_position.Set(y: Neck.GlobalPosition.Y);
        var y_dir = y_position.DirectionTo(Neck.GlobalPosition);
        var y_angle = Mathf.RadToDeg(Neck.GlobalBasis.Z.SignedAngleTo(y_dir, Vector3.Up));
        var y_angle_lerp = Mathf.Lerp(0, y_angle, _look_at_speed * GameTime.DeltaTime);
        if (Mathf.Abs(y_angle) > 1f)
        {
            Neck.RotateY(y_angle_lerp);
        }

        /* UNFINISHED
        var x_position = (Camera.GlobalBasis.Z * target_distance).Set(y: target_position.Y);
        var x_dir = Camera.GlobalPosition.DirectionTo(x_position);
        var x_angle = Mathf.RadToDeg(Camera.GlobalBasis.Z.SignedAngleTo(x_dir, Camera.GlobalBasis.X));
        var x_angle_lerp = Mathf.Lerp(0, x_angle, _look_at_speed * GameTime.DeltaTime);
        if (Mathf.Abs(x_angle) > 5f)
        {
            Camera.RotateX(x_angle_lerp);
        }

        Debug.Log(x_angle);
        */
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

        Vector3 direction = (Neck.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
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
            if (PlayerInput.Jump.Pressed)
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
        if (InteractLock.IsLocked)
        {
            Cursor.Hide();
        }
        else if (Interact?.CurrentInteractable == null)
        {
            Cursor.Hide();
        }
        else if ((Interact?.CurrentInteractable as ITouchable) != null)
        {
            Cursor.Position(Interact.CurrentInteractable.Node);
            Cursor.Show(CursorType.Look, Interact.CurrentInteractable.InteractableText);
        }
        else if (Grab?.IsGrabbing ?? false)
        {
            Cursor.Hide();
        }
        else if (Grab?.CanGrab(Interact?.CurrentInteractable as IGrabbable) ?? false)
        {
            Cursor.Position(Interact.CurrentInteractable.Node);
            Cursor.Show(CursorType.Grab, Interact.CurrentInteractable.InteractableText);
        }
        else
        {
            Cursor.Hide();
        }
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

    private bool TryGrab(IInteractable interactable)
    {
        if (Grab == null) return false;
        if (interactable == null) return false;

        var grabbable = interactable as IGrabbable;
        if (grabbable == null) return false;

        Grab.Grab(grabbable);
        return true;
    }

    private bool TryTouch(IInteractable interactable)
    {
        if (interactable == null) return false;

        var touchable = interactable as ITouchable;
        if (touchable == null) return false;

        touchable.Touch();
        return true;
    }

    public void SetLookRotation(Node3D node)
    {
        Camera.Rotation = new Vector3(node.GlobalRotation.X, 0, 0);
        Neck.Rotation = new Vector3(0, node.GlobalRotation.Y, 0);
    }

    public void UpdateData()
    {
        Data.Game.PlayerPositionX = GlobalPosition.X;
        Data.Game.PlayerPositionY = GlobalPosition.Y;
        Data.Game.PlayerPositionZ = GlobalPosition.Z;
        Data.Game.PlayerCameraRotation = Camera.Rotation.X;
        Data.Game.PlayerNeckRotation = Neck.Rotation.Y;
    }

    public void LoadData()
    {
        GlobalPosition = new Vector3(Data.Game.PlayerPositionX, Data.Game.PlayerPositionY, Data.Game.PlayerPositionZ);
        Camera.Rotation = new Vector3(Data.Game.PlayerCameraRotation, 0, 0);
        Neck.Rotation = new Vector3(0, Data.Game.PlayerNeckRotation, 0);
    }

    public void StartLookingAt(Node3D target, float speed)
    {
        _look_at_target = target;
        _look_at_speed = speed;
    }

    public void StopLookingAt()
    {
        _look_at_target = null;
    }
}

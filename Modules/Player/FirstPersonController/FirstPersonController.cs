using Godot;

public partial class FirstPersonController : CharacterBody3D
{
    public static FirstPersonController Instance { get; private set; }

    [Export]
    public float MoveSpeed = 5.0f;

    [Export]
    public float JumpUpSpeed = 3f;

    [Export]
    public float JumpHorizontalSpeed;

    // Get the gravity from the project settings to be synced with RigidBody nodes.
    public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

    [NodeName("Neck")]
    public Node3D Neck;

    [NodeName("Camera3D")]
    public Camera3D Camera;

    [NodeType(typeof(IPlayerInteract))]
    public IPlayerInteract Interact;

    [NodeType(typeof(IPlayerGrab))]
    public IPlayerGrab Grab;

    public MultiLock InteractLock = new MultiLock();
    public MultiLock MovementLock = new MultiLock();

    public override void _Ready()
    {
        base._Ready();
        Instance = this;
        NodeScript.FindNodesFromAttribute(this, GetType());

        LoadData();
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
    }

    private void Input_RotateView(InputEvent e)
    {
        if (MovementLock.IsLocked) return;
        if (Input.MouseMode != Input.MouseModeEnum.Captured) return;
        if (e is not InputEventMouseMotion) return;
        if (Grab?.IsRotating ?? false) return;

        var motion = e as InputEventMouseMotion;
        Neck.RotateY(-motion.Relative.X * 0.001f);
        Camera.RotateX(-motion.Relative.Y * 0.001f);

        var x = Mathf.Clamp(Camera.Rotation.X, Mathf.DegToRad(-70), Mathf.DegToRad(70));
        Camera.Rotation = Rotation with { X = x };
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
            var has_direction = direction != Vector3.Zero;
            if (has_direction)
            {
                velocity.X = direction.X * MoveSpeed;
                velocity.Z = direction.Z * MoveSpeed;
            }
            else
            {
                velocity.X = Mathf.MoveToward(Velocity.X, 0, MoveSpeed);
                velocity.Z = Mathf.MoveToward(Velocity.Z, 0, MoveSpeed);
            }

            // Handle Jump.
            if (PlayerInput.Jump.Pressed)
            {
                var dir = new Vector3(direction.X, 0, direction.Z).Normalized() * JumpHorizontalSpeed;
                var jump_vel = new Vector3(dir.X, JumpUpSpeed, dir.Z);
                velocity += jump_vel;
            }
        }

        Velocity = velocity;
        MoveAndSlide();
        UpdateData();
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
}

using Godot;

public partial class FirstPersonController : CharacterBody3D
{
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

    public override void _Ready()
    {
        base._Ready();
        NodeScript.FindNodesFromAttribute(this, GetType());
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        RotateView(@event);
        InputMouse(@event);
    }

    private void InputMouse(InputEvent @event)
    {
        var e = @event as InputEventMouseButton;
        if (e == null) return;
        if (e.ButtonIndex == MouseButton.Left)
        {
            if (e.IsPressed())
            {
                var interactable = Interact.CurrentInteractable;
                if (TryGrab(interactable)) return;
            }
            else if (e.IsReleased())
            {
                Grab?.Release();
            }
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

    private void RotateView(InputEvent e)
    {
        if (Input.MouseMode != Input.MouseModeEnum.Captured) return;
        if (e is not InputEventMouseMotion) return;

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
        // As good practice, you should replace UI actions with custom gameplay actions.
        Vector2 inputDir = Input.GetVector("Left", "Right", "Forward", "Back");
        Vector3 direction = (Neck.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

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
            if (Input.IsActionJustPressed("Jump"))
            {
                var dir = new Vector3(direction.X, 0, direction.Z).Normalized() * JumpHorizontalSpeed;
                var jump_vel = new Vector3(dir.X, JumpUpSpeed, dir.Z);
                velocity += jump_vel;
            }
        }

        Velocity = velocity;
        MoveAndSlide();
    }
}

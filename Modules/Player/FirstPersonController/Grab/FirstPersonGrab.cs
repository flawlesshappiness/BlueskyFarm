using Godot;

public partial class FirstPersonGrab : Node3DScript
{
    [Export]
    public float GrabPositionOffsetMax;

    [NodeName("Position")]
    public Node3D PositionNode;

    [NodeName("Rotation")]
    public Node3D RotationNode;

    public Grabbable Target { get; private set; }
    public Vector3 GrabPosition => PositionNode.GlobalPosition + GrabOffset;
    public Vector3 GrabOffset => PositionNode.GlobalBasis * Vector3.Forward * GrabOffsetLength;
    public float GrabOffsetLength { get; private set; }
    public Vector3 GrabRotation => RotationNode.GlobalRotation;
    public bool IsGrabbing => Target != null;
    public bool IsRotating => IsGrabbing && PlayerInput.Rotate.Held;

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        Input_Rotate(@event);
    }

    private void Input_Rotate(InputEvent e)
    {
        if (Input.MouseMode != Input.MouseModeEnum.Captured) return;
        if (e is not InputEventMouseMotion) return;
        if (!IsRotating) return;

        var motion = e as InputEventMouseMotion;
        var x = motion.Relative.X * 0.005f;
        var y = motion.Relative.Y * 0.005f;

        RotationNode.GlobalRotate(PositionNode.GlobalBasis[1], x);
        RotationNode.GlobalRotate(PositionNode.GlobalBasis[0], y);
    }

    private void CalculateGrabRotationOffset(Grabbable target)
    {
        RotationNode.GlobalRotation = target.GlobalRotation;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        Process_Target();
        Process_GrabOffset();
    }

    private void Process_GrabOffset()
    {
        if (PlayerInput.ZoomIn.Pressed)
        {
            SetGrabOffset(GrabOffsetLength + 0.2f);
        }
        else if (PlayerInput.ZoomOut.Pressed)
        {
            SetGrabOffset(GrabOffsetLength - 0.2f);
        }
    }

    private void SetGrabOffset(float offset)
    {
        GrabOffsetLength = Mathf.Clamp(offset, 0, GrabPositionOffsetMax);
    }

    private void CalculateGrabOffset(Grabbable target)
    {
        var distance = PositionNode.GlobalPosition.DistanceTo(target.GlobalPosition);
        SetGrabOffset(distance);
    }

    private void Process_Target()
    {
        if (Target == null) return;
        if (PositionNode == null) return;
        if (!CanGrab(Target))
        {
            Release();
            return;
        }

        Target.TargetPosition = GrabPosition;
        Target.TargetRotation = GrabRotation;
    }

    public void SetTarget(Grabbable grabbable)
    {
        if (grabbable == null) return;
        if (!CanGrab(grabbable)) return;

        CalculateGrabOffset(grabbable);
        CalculateGrabRotationOffset(grabbable);

        Target = grabbable;
        Target.Grabbed();
    }

    public void RemoveTarget()
    {
        if (Target == null) return;

        if (IsInstanceValid(Target))
        {
            Target.Released();
        }

        Target = null;
    }

    public Grabbable GetCurrentTarget()
    {
        return Target;
    }

    public void Grab(Grabbable grabbable)
    {
        SetTarget(grabbable);
    }

    public void Release()
    {
        RemoveTarget();
    }

    public bool CanGrab(Grabbable grabbable)
    {
        if (grabbable == null) return false;
        if (!IsInstanceValid(grabbable)) return false;
        if (grabbable.IsQueuedForDeletion()) return false;
        if (!grabbable.IsGrabbable) return false;
        return true;
    }

    public bool TryHandleCursor(IInteractable interactable)
    {
        if (Target != null)
        {
            Cursor.Hide();
            return true;
        }

        var grabbable = interactable as Grabbable;
        if (!IsInstanceValid(grabbable)) return false;
        if (!CanGrab(grabbable)) return false;

        Cursor.Show(new CursorSettings
        {
            Name = "Grab",
            Text = interactable.GetHoverText(),
            Position = interactable.Body.GlobalPosition
        });

        return true;
    }
}

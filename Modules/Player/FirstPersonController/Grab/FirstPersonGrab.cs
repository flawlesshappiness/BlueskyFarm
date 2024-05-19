using Godot;

public partial class FirstPersonGrab : Node3DScript, IPlayerGrab
{
    [Export]
    public float GrabPositionOffsetMax;

    [NodeName("Position")]
    public Node3D GrabNode;

    public IGrabbable Target { get; private set; }
    public Vector3 GrabPosition => GrabNode.GlobalPosition + GrabOffset;
    public Vector3 GrabOffset => GrabNode.GlobalBasis * Vector3.Forward * GrabOffsetLength;
    public float GrabOffsetLength { get; private set; }
    public bool IsGrabbing => Target != null;

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        InputScrollOffset(@event);
    }

    private void InputScrollOffset(InputEvent @event)
    {
        var e = @event as InputEventMouseButton;
        if (e == null) return;
        if (!e.Pressed) return;

        if (e.ButtonIndex == MouseButton.WheelUp)
        {
            SetGrabOffset(GrabOffsetLength + 0.2f);
        }
        else if (e.ButtonIndex == MouseButton.WheelDown)
        {
            SetGrabOffset(GrabOffsetLength - 0.2f);
        }
    }

    private void SetGrabOffset(float offset)
    {
        GrabOffsetLength = Mathf.Clamp(offset, 0, GrabPositionOffsetMax);
    }

    private void CalculateGrabOffset(IGrabbable target)
    {
        var distance = GrabNode.GlobalPosition.DistanceTo(target.Node.GlobalPosition);
        SetGrabOffset(distance);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        ProcessTarget();
    }

    private void ProcessTarget()
    {
        if (Target == null) return;
        if (GrabNode == null) return;

        Target.SetPosition(GrabPosition);
    }

    public void SetTarget(IGrabbable grabbable)
    {
        if (grabbable == null) return;

        CalculateGrabOffset(grabbable);

        Target = grabbable;
        Target.Grabbed();
    }

    public void RemoveTarget()
    {
        if (Target == null) return;

        Target.Released();
        Target = null;
    }

    public void Grab(IGrabbable grabbable)
    {
        SetTarget(grabbable);
    }

    public void Release()
    {
        RemoveTarget();
    }
}

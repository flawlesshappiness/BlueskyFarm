using Godot;

public partial class FirstPersonGrab : Node3DScript, IPlayerGrab
{
    [NodeName("Position")]
    public Node3D GrabNode;

    public IGrabbable Target { get; private set; }

    public Vector3 GrabPosition => GrabNode.GlobalPosition;

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

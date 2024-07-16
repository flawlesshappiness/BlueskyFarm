using Godot;

public partial class SporeMushroomCluster : Node3DScript
{
    [NodeType]
    public PsMushroomSmoke PsSmoke;

    [NodeName]
    public Area3D Trigger;

    private bool _triggered;

    public override void _Ready()
    {
        base._Ready();
        Hide();

        Trigger.BodyEntered += TriggerEntered;
    }

    private void TriggerEntered(Node3D body)
    {
        if (!Visible) return;
        if (_triggered) return;

        PsSmoke.PlayPuff();
        PsSmoke.PlayIdle();
        _triggered = true;
    }
}

using Godot;

public partial class WispEnemy : NavEnemy
{
    [Export]
    public Node3D AnimNode;

    [Export]
    public Path3D Path;

    private float _current_distance;
    private Vector3 _target_position;

    private enum State { Move, Wait }

    protected override void Initialize()
    {
        base.Initialize();
        Spawn();
    }

    public override void Spawn(bool debug)
    {
        base.Spawn(debug);
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        ProcessPath();
        ProcessPosition();
    }

    private void ProcessPath()
    {
        var length = Path.Curve.GetBakedLength();
        var speed = 0.5f * GameTime.DeltaTime;
        _current_distance = (_current_distance + speed) % length;
        _target_position = Path.GlobalPosition + Path.Curve.SampleBaked(offset: _current_distance);
    }

    private void ProcessPosition()
    {
        AnimNode.GlobalPosition = AnimNode.GlobalPosition.Lerp(_target_position, 0.5f * GameTime.DeltaTime);
    }
}

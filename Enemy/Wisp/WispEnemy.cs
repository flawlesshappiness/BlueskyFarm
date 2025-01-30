using Godot;
using System;

public partial class WispEnemy : NavEnemy
{
    [Export]
    public Node3D AnimNode;

    [Export]
    public Path3D Path;

    private float _current_distance;

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        var fdelta = Convert.ToSingle(delta);
        ProcessPath(fdelta);
    }

    private void ProcessPath(float delta)
    {
        var length = Path.Curve.GetBakedLength();
        var speed = 0.5f * delta;
        _current_distance = (_current_distance + speed) % length;
        var position = Path.GlobalPosition + Path.Curve.SampleBaked(offset: _current_distance);
        AnimNode.GlobalPosition = AnimNode.GlobalPosition.Lerp(position, 0.5f * delta);
    }
}

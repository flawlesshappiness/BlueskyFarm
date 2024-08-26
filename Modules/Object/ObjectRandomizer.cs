using Godot;

public partial class ObjectRandomizer : Node3DScript
{
    [NodeName]
    public Node3D Position;

    [NodeName]
    public Node3D Models;

    [Export]
    public float PositionRadiusMin = 0f;

    [Export]
    public float PositionRadiusMax = 0f;

    [Export]
    public float RotationMin = 0;

    [Export]
    public float RotationMax = 0f;

    [Export]
    public float ScaleMin = 1f;

    [Export]
    public float ScaleMax = 1f;

    private RandomNumberGenerator _rng = new RandomNumberGenerator();

    public override void _Ready()
    {
        base._Ready();
        RandomizeModel();
        RandomizePosition();
        RandomizeRotation();
        RandomizeScale();
    }

    public void RandomizePosition()
    {
        var r = _rng.RandfRange(PositionRadiusMin, PositionRadiusMax);
        var x = _rng.Randf();
        var z = _rng.Randf();
        var p = new Vector3(x, 0, z).Normalized() * r;
        Position.Position = p;
    }

    public void RandomizeRotation()
    {
        var r = _rng.RandfRange(RotationMin, RotationMax);
        Position.RotationDegrees = new Vector3(0, r, 0);
    }

    public void RandomizeScale()
    {
        var s = _rng.RandfRange(ScaleMin, ScaleMax);
        Position.Scale = Vector3.One * s;
    }

    public void RandomizeModel()
    {
        var children = Models.GetChildren();
        var idx = _rng.RandiRange(0, children.Count - 1);
        for (int i = 0; i < children.Count; i++)
        {
            var child = children[i] as Node3D;
            if (child == null) continue;

            child.Visible = i == idx;
        }
    }
}

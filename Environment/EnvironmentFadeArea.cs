using Godot;

public partial class EnvironmentFadeArea : Node3DScript
{
    [Export]
    public Environment EnvStart;

    [Export]
    public Environment EnvEnd;

    [NodeType]
    public Area3D Area;

    [NodeName]
    public Node3D Start;

    [NodeName]
    public Node3D End;

    private bool _tracking;
    private EnvironmentLerp _lerp;

    public override void _Ready()
    {
        base._Ready();

        Area.BodyEntered += v => CallDeferred(nameof(BodyEntered), v);
        Area.BodyExited += v => CallDeferred(nameof(BodyExited), v);
    }

    private void BodyEntered(GodotObject obj)
    {
        var player = obj as FirstPersonController;
        if (player == null) return;

        _tracking = true;

        var scene = Scene.Current as GameScene;
        if (scene == null) return;

        _lerp = EnvStart.CreateLerp(EnvEnd);
        _lerp.UpdateLerp(GetLerpValue());
        scene.WorldEnvironment.Environment = _lerp.Result;
    }

    private void BodyExited(GodotObject obj)
    {
        var player = obj as FirstPersonController;
        if (player == null) return;

        _tracking = false;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        Process_Tracking();
    }

    private void Process_Tracking()
    {
        if (!_tracking) return;
        var t = GetLerpValue();
        _lerp.UpdateLerp(t);
    }

    private float GetLerpValue()
    {
        var p = Player.Instance.GlobalPosition;
        var a = p - Start.GlobalPosition;
        var b = End.GlobalPosition - Start.GlobalPosition;
        var angle = a.AngleTo(b);
        var cos = Mathf.Cos(angle);
        var v = (a.Length() * cos) / b.Length();
        return Mathf.Clamp(v, 0, 1);
    }
}

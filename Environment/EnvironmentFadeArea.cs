using Godot;

public partial class EnvironmentFadeArea : Node3DScript
{
    [Export]
    public AreaNameType StartArea;

    [Export]
    public AreaNameType EndArea;

    [NodeType]
    public Area3D Area;

    [NodeName]
    public Node3D Start;

    [NodeName]
    public Node3D End;

    private bool _tracking;
    private EnvironmentLerp _env_lerp;
    private EnvironmentInfo _info_start;
    private EnvironmentInfo _info_end;
    private GameScene _scene;

    public override void _Ready()
    {
        base._Ready();

        Area.BodyEntered += v => CallDeferred(nameof(BodyEntered), v);
        Area.BodyExited += v => CallDeferred(nameof(BodyExited), v);

        OptionsController.Instance.OnBrightnessChanged += BrightnessChanged;

        _info_start = EnvironmentController.Instance.GetInfo(StartArea);
        _info_end = EnvironmentController.Instance.GetInfo(EndArea);

        _scene = Scene.Current as GameScene;
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        OptionsController.Instance.OnBrightnessChanged -= BrightnessChanged;
    }

    private void BodyEntered(GodotObject obj)
    {
        var player = obj as FirstPersonController;
        if (player == null) return;

        _tracking = true;
        CreateLerp();
    }

    private void CreateLerp()
    {
        _env_lerp = _info_start.Environment.CreateLerp(_info_end.Environment);
        _env_lerp.UpdateLerp(GetLerpValue());
        _scene.WorldEnvironment.Environment = _env_lerp.Result;
    }

    private void BrightnessChanged()
    {
        _info_start.Environment.AdjustmentBrightness = Data.Options.Brightness;
        _info_end.Environment.AdjustmentBrightness = Data.Options.Brightness;

        if (_tracking)
        {
            CreateLerp();
        }
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
        _env_lerp.UpdateLerp(t);

        _scene.DirectionalLight.LightColor = _info_start.DirectionalLightColor.Lerp(_info_end.DirectionalLightColor, t);
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

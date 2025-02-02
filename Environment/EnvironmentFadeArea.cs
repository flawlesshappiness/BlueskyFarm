using Godot;
using System.Collections;

[GlobalClass]
public partial class EnvironmentFadeArea : PlayerArea
{
    [Export]
    public AreaNameType StartArea;

    [Export]
    public AreaNameType EndArea;

    [Export]
    public Node3D Start;

    [Export]
    public Node3D End;

    private bool _tracking;
    private float _lerp_value;
    private Coroutine _cr_lerp;
    private EnvironmentLerp _env_lerp;
    private EnvironmentInfo _info_start;
    private EnvironmentInfo _info_end;
    private GameScene _scene;

    public override void _Ready()
    {
        base._Ready();

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

    protected override void PlayerEntered(Player player)
    {
        base.PlayerEntered(player);
        _tracking = true;
        CreateLerp();
    }

    protected override void PlayerExited(Player player)
    {
        base.PlayerExited(player);
        _tracking = false;
        LerpToEndValue();
    }

    private void CreateLerp()
    {
        Coroutine.Stop(_cr_lerp);
        _env_lerp = _info_start.Environment.CreateLerp(_info_end.Environment);
        _scene.WorldEnvironment.Environment = _env_lerp.Result;

        _lerp_value = GetLerpValue();
        UpdateLerp(_lerp_value);
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

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        Process_Tracking();
    }

    private void Process_Tracking()
    {
        if (!_tracking) return;
        _lerp_value = GetLerpValue();
        UpdateLerp(_lerp_value);
    }

    private void UpdateLerp(float t)
    {
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

    private void LerpToEndValue()
    {
        var t_current = _lerp_value;
        var t_end = t_current > 0.5f ? 1f : 0f;

        Coroutine.Stop(_cr_lerp);
        _cr_lerp = Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            var curve = Curves.EaseOutQuad;
            yield return LerpEnumerator.Lerp01(0.25f, f =>
            {
                var t_curve = curve.Evaluate(f);
                var t = Mathf.Lerp(t_current, t_end, t_curve);
                UpdateLerp(t);
            });

            UpdateLerp(t_end);
        };
    }
}

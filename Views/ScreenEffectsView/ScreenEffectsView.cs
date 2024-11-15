using Godot;

public partial class ScreenEffectsView : View
{
    public override string Directory => $"{Paths.ViewDirectory}/{nameof(ScreenEffectsView)}";

    [NodeName]
    public ColorRect Radial;

    [NodeName]
    public ColorRect Gaussian;

    [NodeName]
    public ColorRect Distort;

    [NodeName]
    public ColorRect AnimatedFog;

    [NodeName]
    public SubViewport Main;

    [NodeType]
    public Camera3D Camera;

    [NodeName]
    public Node3D Test;

    public ShaderMaterial RadialMaterial => _mat_radial ?? (_mat_radial = Radial.Material as ShaderMaterial);
    private ShaderMaterial _mat_radial;

    public ShaderMaterial GaussianMaterial => _mat_gaussian ?? (_mat_gaussian = Gaussian.Material as ShaderMaterial);
    private ShaderMaterial _mat_gaussian;

    public ShaderMaterial DistortMaterial => _mat_distort ?? (_mat_distort = Distort.Material as ShaderMaterial);
    private ShaderMaterial _mat_distort;

    public ShaderMaterial FogMaterial => _mat_fog ?? (_mat_fog = AnimatedFog.Material as ShaderMaterial);
    private ShaderMaterial _mat_fog;

    private Node3D _camera_target;

    public void Clear()
    {
        GaussianBlurAmount = 0;
        RadialBlurAmount = 0;
        DistortStrength = 0;
        Fog_Alpha = 0;

        Test.Hide();
    }

    public float DistortStrength
    {
        get => DistortMaterial.GetShaderParameter("strength").AsSingle();
        set => DistortMaterial.SetShaderParameter("strength", value);
    }

    public float DistortSpeed
    {
        get => DistortMaterial.GetShaderParameter("speed").AsSingle();
        set => DistortMaterial.SetShaderParameter("speed", value);
    }

    public Vector2 DistortDisplacement
    {
        get => DistortMaterial.GetShaderParameter("displacement").AsVector2();
        set => DistortMaterial.SetShaderParameter("displacement", value);
    }

    public float GaussianBlurAmount
    {
        get => GaussianMaterial.GetShaderParameter("blur_amount").AsSingle();
        set => GaussianMaterial.SetShaderParameter("blur_amount", value);
    }

    public Vector2 RadialBlurCenter
    {
        get => RadialMaterial.GetShaderParameter("blur_center").AsVector2();
        set => RadialMaterial.SetShaderParameter("blur_center", value);
    }

    public float RadialBlurAmount
    {
        get => RadialMaterial.GetShaderParameter("blur_power").AsSingle();
        set => RadialMaterial.SetShaderParameter("blur_power", value);
    }

    public int RadialBlurSamplingCount
    {
        get => RadialMaterial.GetShaderParameter("sampling_count").AsInt32();
        set => RadialMaterial.SetShaderParameter("sampling_count", value);
    }

    public float Fog_Alpha
    {
        get => Fog_Color.A;
        set => Fog_Color = new Color(Fog_Color.R, Fog_Color.G, Fog_Color.B, value);
    }

    public Color Fog_Color
    {
        get => FogMaterial.GetShaderParameter("color").AsColor();
        set => FogMaterial.SetShaderParameter("color", value);
    }

    public Vector2 Fog_Direction
    {
        get => FogMaterial.GetShaderParameter("fog_direction").AsVector2();
        set => FogMaterial.SetShaderParameter("fog_direction", value);
    }

    public override void _Ready()
    {
        base._Ready();
        Visible = true;
        Clear();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        Process_MatchCamera();
    }

    private void Process_MatchCamera()
    {
        if (IsInstanceValid(_camera_target))
        {
            Camera.GlobalTransform = _camera_target.GlobalTransform;
        }
    }

    public void SetCameraTarget(Node3D target)
    {
        _camera_target = target;
    }
}

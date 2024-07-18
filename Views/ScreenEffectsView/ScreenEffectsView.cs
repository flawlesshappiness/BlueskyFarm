using Godot;

public partial class ScreenEffectsView : View
{
    public override string Directory => $"{Paths.ViewDirectory}/{nameof(ScreenEffectsView)}";

    [NodeName]
    public ColorRect Combined;

    public ShaderMaterial CombinedMaterial => _mat_combined ?? (_mat_combined = Combined.Material as ShaderMaterial);
    private ShaderMaterial _mat_combined;

    public float Distort_Strength
    {
        get => CombinedMaterial.GetShaderParameter("distort_strength").AsSingle();
        set => CombinedMaterial.SetShaderParameter("distort_strength", value);
    }

    public float Distort_Speed
    {
        get => CombinedMaterial.GetShaderParameter("distort_speed").AsSingle();
        set => CombinedMaterial.SetShaderParameter("distort_speed", value);
    }

    public Vector2 Distort_Displacement
    {
        get => CombinedMaterial.GetShaderParameter("distort_displacement").AsVector2();
        set => CombinedMaterial.SetShaderParameter("distort_displacement", value);
    }

    public int Blur_Type
    {
        get => CombinedMaterial.GetShaderParameter("blur_type").AsInt32();
        set => CombinedMaterial.SetShaderParameter("blur_type", value);
    }

    public int Blur_Amount
    {
        get => CombinedMaterial.GetShaderParameter("blur_amount").AsInt32();
        set => CombinedMaterial.SetShaderParameter("blur_amount", value);
    }

    public Vector2 Radial_Center
    {
        get => CombinedMaterial.GetShaderParameter("radial_center").AsVector2();
        set => CombinedMaterial.SetShaderParameter("radial_center", value);
    }

    public float Radial_Strength
    {
        get => CombinedMaterial.GetShaderParameter("radial_strength").AsSingle();
        set => CombinedMaterial.SetShaderParameter("radial_strength", value);
    }

    public int Radial_Sampling
    {
        get => CombinedMaterial.GetShaderParameter("radial_sampling").AsInt32();
        set => CombinedMaterial.SetShaderParameter("radial_sampling", value);
    }

    public override void _Ready()
    {
        base._Ready();
        Visible = true;

        Distort_Strength = 0;
        Blur_Amount = 0;
        Radial_Strength = 0;
    }
}

using Godot;

[GlobalClass]
public partial class EnvironmentInfo : Resource
{
    [Export]
    public AreaNameType Area;

    [Export]
    public Environment Environment;

    [Export]
    public Color DirectionalLightColor;
}

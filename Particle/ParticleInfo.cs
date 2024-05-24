using Godot;

[GlobalClass]
public partial class ParticleInfo : Resource
{
    [Export(PropertyHint.File)]
    public string Path { get; set; }

    [Export]
    public ParticleType Type { get; set; }
}

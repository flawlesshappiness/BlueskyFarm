using Godot;
using Godot.Collections;

[GlobalClass]
public partial class AmbientParticleInfo : Resource
{
    [Export]
    public string AreaName;

    [Export]
    public float DelayMin = 5f;

    [Export]
    public float DelayMax = 10f;

    [Export]
    public float DistanceMin = 12;

    [Export]
    public float DistanceMax = 20;

    [Export]
    public Array<string> Particles;
}

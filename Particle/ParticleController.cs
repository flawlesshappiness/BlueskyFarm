using Godot;

public partial class ParticleController : ResourceController<ParticleCollection, ParticleInfo>
{
    public static ParticleController Instance => Singleton.Get<ParticleController>();
    public override string Directory => "Particle";

    public GpuParticles3D CreateParticle(string name)
    {
        var info = Collection.GetResource(name);
        return CreateParticle(info);
    }

    public GpuParticles3D CreateParticle(ParticleInfo info)
    {
        var ps = info.Scene.Instantiate<GpuParticles3D>();
        ps.SetParent(Scene.Current);
        return ps;
    }
}

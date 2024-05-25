using Godot;
using System.Linq;

public partial class ParticleController : ResourceController<ParticleCollection, ParticleInfo>
{
    public static ParticleController Instance => Singleton.Get<ParticleController>();
    public override string Directory => "Particle";

    public GpuParticles3D CreateParticle(ParticleType type)
    {
        var info = Collection.Resources.FirstOrDefault(x => x.Type == type);
        return GDHelper.Instantiate<GpuParticles3D>(info.Path);
    }
}

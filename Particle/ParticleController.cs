using Godot;
using System.Linq;

public partial class ParticleController : ResourceController<ParticleCollection, ParticleInfo>
{
    public static ParticleController Instance => GetController<ParticleController>("Particle");
    public ParticleCollection Collection => GetCollection("Particle/Resources/ParticleCollection.tres");

    public GpuParticles3D CreateParticle(ParticleType type)
    {
        var info = Collection.Resources.FirstOrDefault(x => x.Type == type);
        return GDHelper.Instantiate<GpuParticles3D>(info.Path);
    }
}

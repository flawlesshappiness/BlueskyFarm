using Godot;
using Godot.Collections;
using System.IO;

public partial class ParticleController : ResourceController<ParticleCollection, ParticleInfo>
{
    public static ParticleController Instance => Singleton.Get<ParticleController>();
    public override string Directory => "Particle";

    private Dictionary<string, ParticleInfo> _particles = new();

    public override void _Ready()
    {
        base._Ready();

        foreach (var info in Collection.Resources)
        {
            var name = Path.GetFileNameWithoutExtension(info.ResourcePath);
            _particles.Add(name, info);
        }
    }

    public GpuParticles3D CreateParticle(string name)
    {
        if (_particles.ContainsKey(name))
        {
            var info = _particles[name];
            return GDHelper.Instantiate<GpuParticles3D>(info.Path);
        }

        Debug.LogError($"Failed to create particle with name: {name}");
        return null;
    }
}

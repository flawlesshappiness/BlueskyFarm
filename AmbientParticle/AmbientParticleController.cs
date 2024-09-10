using Godot;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class AmbientParticleController : ResourceController<AmbientParticleCollection, AmbientParticleInfo>
{
    public static AmbientParticleController Instance => Singleton.Get<AmbientParticleController>();
    public override string Directory => "AmbientParticle";

    private List<Coroutine> _coroutines = new();

    public void Start(string area_name)
    {
        Stop();

        var infos = Collection.Resources.Where(x => x.AreaName == area_name);
        foreach (var info in infos)
        {
            var cr = Coroutine.Start(CrAmbience(info));
            _coroutines.Add(cr);
        }
    }

    public void Stop()
    {
        foreach (var cr in _coroutines)
        {
            Coroutine.Stop(cr);
        }

        _coroutines.Clear();
    }

    private IEnumerator CrAmbience(AmbientParticleInfo info)
    {
        var rng = new RandomNumberGenerator();
        var mul_debug = 0.2f;
        yield return new WaitForSeconds(rng.RandfRange(15, 30) * mul_debug);

        while (true)
        {
            var particle_name = info.Particles
                .Where(x => info.Particles.Count == 1)
                .ToList()
                .Random();
            var x = rng.RandfRange(-1, 1);
            var z = rng.RandfRange(-1, 1);
            var distance = rng.RandfRange(info.DistanceMin, info.DistanceMax);
            var offset = new Vector3(x, 0, z).Normalized() * distance;
            var position = FirstPersonController.Instance.GlobalPosition + offset;

            Particle.PlayOneShot(particle_name, position);
            Debug.Log("TEST");

            var delay = rng.RandfRange(15, 30) * mul_debug;
            yield return new WaitForSeconds(delay);
        }
    }
}

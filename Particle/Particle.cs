using Godot;
using System;
using System.Collections;

public static class Particle
{
    public static GpuParticles3D PlayOneShot(string name, Vector3 position)
    {
        var info = ParticleController.Instance.Collection.GetResource(name);
        return PlayOneShot(info, position);
    }

    public static GpuParticles3D PlayOneShot(ParticleInfo info, Vector3 position)
    {
        var ps = ParticleController.Instance.CreateParticle(info);
        if (!GodotObject.IsInstanceValid(ps)) return null;

        ps.GlobalPosition = position;
        ps.OneShot = true;
        ps.Emitting = true;
        DestroyParticleAfterLifetime(ps);
        return ps;
    }

    private static void DestroyParticleAfterLifetime(GpuParticles3D particle)
    {
        var particles_per_second = particle.Amount * particle.AmountRatio / particle.Lifetime;
        var particle_spawn_time = 1f / particles_per_second * particle.Amount;
        var delay = particle.Lifetime + particle_spawn_time;
        DestroyParticleAfterDelay(particle, Convert.ToSingle(delay));
    }

    private static void DestroyParticleAfterDelay(GpuParticles3D particle, float delay)
    {
        Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            yield return new WaitForSeconds(delay);

            if (GodotObject.IsInstanceValid(particle) && !particle.IsQueuedForDeletion())
            {
                particle.QueueFree();
            }
        }
    }
}

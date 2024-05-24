using Godot;
using System;
using System.Collections;

public static class Particle
{
    public static GpuParticles3D PlayOneShot(ParticleType type, Vector3 position)
    {
        var ps = ParticleController.Instance.CreateParticle(type);
        ps.GlobalPosition = position;
        ps.OneShot = true;
        ps.Emitting = true;
        DestroyParticleAfterLifetime(ps);
        return ps;
    }

    private static void DestroyParticleAfterLifetime(GpuParticles3D particle)
    {
        DestroyParticleAfterDelay(particle, Convert.ToSingle(particle.Lifetime));
    }

    private static void DestroyParticleAfterDelay(GpuParticles3D particle, float delay)
    {
        Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            yield return new WaitForSeconds(delay);
            particle.QueueFree();
        }
    }
}

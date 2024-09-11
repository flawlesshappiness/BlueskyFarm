using Godot;
using System.Collections.Generic;

public partial class SteamPuzzle : Node3DScript
{
    [NodeName]
    public Node3D Steam;

    protected IEnumerable<GpuParticles3D> _steam_particles;
    protected IEnumerable<AudioStreamPlayer3D> _steam_sounds;
    protected IEnumerable<CollisionShape3D> _steam_collisions;

    public override void _Ready()
    {
        base._Ready();
        _steam_particles = Steam.GetNodesInChildren<GpuParticles3D>();
        _steam_sounds = Steam.GetNodesInChildren<AudioStreamPlayer3D>();
        _steam_collisions = Steam.GetNodesInChildren<CollisionShape3D>();
    }

    protected void StartSteam()
    {
        _steam_particles.ForEach(x => x.Emitting = true);
        _steam_sounds.ForEach(x => x.Playing = true);
        _steam_collisions.ForEach(x => x.Disabled = false);
    }

    protected void StopSteam()
    {
        _steam_particles.ForEach(x => x.Emitting = false);
        _steam_sounds.ForEach(x => x.Stop());
        _steam_collisions.ForEach(x => x.Disabled = true);
    }
}

using Godot;

public partial class CultHoleFire : Node3D
{
    [Export]
    public AnimationPlayer AnimationPlayer_Light;

    [Export]
    public OmniLight3D Light_Fire;

    [Export]
    public GpuParticles3D PsFlames;

    [Export]
    public GpuParticles3D PsEmbers;

    [Export]
    public AudioStreamPlayer3D SfxFire;

    public override void _Ready()
    {
        base._Ready();
        AnimationPlayer_Light.Play("fire");
        Stop();
    }

    public void Stop()
    {
        Light_Fire.Hide();
        PsFlames.Emitting = false;
        PsEmbers.Emitting = false;
        SfxFire.Stop();
    }

    public void Start()
    {
        Light_Fire.Show();
        PsFlames.Emitting = true;
        PsFlames.Emitting = true;
        SfxFire.Play();
    }
}

using Godot;
using Godot.Collections;
using System;

public partial class CrushableRock : Touchable, ICrushable
{
    [Export]
    public SoundInfo sfx_touch;

    [Export]
    public SoundInfo SfxCrush;

    [Export]
    public Array<GpuParticles3D> Particles;

    public event Action OnCrushed;

    public void Crush()
    {
        if (Particles != null)
        {
            Particles.ForEach(x => x.Emitting = true);
        }

        SfxCrush.Play(GlobalPosition);

        OnCrushed?.Invoke();
    }

    protected override void Touched()
    {
        base.Touched();
        SoundController.Instance.Play(sfx_touch, GlobalPosition);
    }
}

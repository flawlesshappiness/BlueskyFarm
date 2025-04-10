using Godot;
using System;

public partial class CrushableRock : Touchable, ICrushable
{
    [Export]
    public SoundInfo sfx_touch;

    public event Action OnCrushed;

    public void Crush()
    {
        OnCrushed?.Invoke();
    }

    protected override void Touched()
    {
        base.Touched();
        SoundController.Instance.Play(sfx_touch, GlobalPosition);
    }
}

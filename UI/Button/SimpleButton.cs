using Godot;
using System.Collections;

public partial class SimpleButton : Button
{
    [Export]
    public SoundInfo HoverSound;

    [Export]
    public SoundInfo PressedSound;

    [Export]
    public Control HoverGraphic;

    public override void _Ready()
    {
        base._Ready();

        Pressed += OnPressed;
        MouseEntered += MouseEnter;
        MouseExited += MouseExit;

        HoverGraphic.Hide();
    }

    protected virtual void OnPressed()
    {
        var asp = SoundController.Instance.Play(PressedSound);
        asp.ProcessMode = ProcessModeEnum.Always;
    }

    protected virtual void MouseEnter()
    {
        var asp = SoundController.Instance.Play(HoverSound);
        asp.ProcessMode = ProcessModeEnum.Always;

        HoverGraphic.Show();
    }

    protected virtual void MouseExit()
    {
        HoverGraphic.Hide();
    }

    private Coroutine AnimateHoverGraphic(bool show, float duration)
    {
        return this.StartCoroutine(Cr, nameof(AnimateHoverGraphic))
            .SetRunWhilePaused(true);
        IEnumerator Cr()
        {
            var start = HoverGraphic.Modulate.A;
            var end = show ? 1f : 0f;
            yield return LerpEnumerator.Lerp01_Unscaled(duration, f =>
            {
                var a = Mathf.Lerp(start, end, f);
                var modulate = HoverGraphic.Modulate.SetA(a);
                HoverGraphic.Modulate = modulate;
            });
        }
    }
}

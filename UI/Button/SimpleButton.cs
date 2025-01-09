using Godot;

public partial class SimpleButton : Button
{
    [Export]
    public SoundInfo HoverSound;

    [Export]
    public SoundInfo PressedSound;

    public override void _Ready()
    {
        base._Ready();

        Pressed += OnPressed;
        MouseEntered += OnHover;
    }

    protected virtual void OnPressed()
    {
        SoundController.Instance.Play(PressedSound);
    }

    protected virtual void OnHover()
    {
        SoundController.Instance.Play(HoverSound);
    }
}

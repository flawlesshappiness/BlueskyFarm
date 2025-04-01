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
        var asp = SoundController.Instance.Play(PressedSound);
        asp.ProcessMode = ProcessModeEnum.Always;
    }

    protected virtual void OnHover()
    {
        var asp = SoundController.Instance.Play(HoverSound);
        asp.ProcessMode = ProcessModeEnum.Always;
    }
}

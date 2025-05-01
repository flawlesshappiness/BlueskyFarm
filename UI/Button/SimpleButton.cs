using Godot;

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
}

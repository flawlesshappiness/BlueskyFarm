using Godot;

public partial class SignDirectionBoard : Node3DScript
{
    [NodeType]
    public Touchable Touchable;

    public string Text { get; set; }

    public override void _Ready()
    {
        base._Ready();
        Touchable.OnTouched += Touched;
    }

    private void Touched()
    {
        SoundController.Instance.Play("sfx_throw_light");

        GameView.Instance.CreateText(new CreateTextSettings
        {
            Id = "sign_direction_" + GetInstanceId(),
            Text = Tr(Text),
            Target = Touchable,
            Offset = new Vector3(0, 0.2f, 0),
            Duration = 5.0f,
        });
    }
}

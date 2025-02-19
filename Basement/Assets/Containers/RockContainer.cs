using Godot;

public partial class RockContainer : ItemContainer
{
    [Export]
    public Node3D ItemMarker;

    [Export]
    public Node3D ModelUnbroken;

    [Export]
    public Node3D ModelBroken;

    [Export]
    public Touchable Touchable;

    [Export]
    public GpuParticles3D ps_dust_break;

    [Export]
    public GpuParticles3D ps_bits_break;

    [Export]
    public SoundInfo sfx_break;

    [Export]
    public SoundInfo sfx_touch;

    private bool _broken;

    public override void _Ready()
    {
        base._Ready();
        Touchable.OnTouched += Touched;

        ModelBroken.Disable();
    }

    public void Break()
    {
        if (_broken) return;
        _broken = true;

        ps_dust_break.Emitting = true;
        ps_bits_break.Emitting = true;

        SoundController.Instance.Play(sfx_break, Touchable.GlobalPosition);

        SpawnItem(ItemMarker.GlobalPosition, Vector3.Up * 2f);
        ModelUnbroken.Disable();
        ModelBroken.Enable();
    }

    private void Touched()
    {
        SoundController.Instance.Play(sfx_touch, Touchable.GlobalPosition);

        GameView.Instance.CreateText(new CreateTextSettings
        {
            Id = "tool_required_" + GetInstanceId(),
            Text = "##TOOL_REQUIRED##",
            Target = Touchable,
            Offset = new Vector3(0, 0.2f, 0),
            Duration = 5.0f,
            Shake_Duration = 0.4f,
            Shake_Frequency = 0.04f,
            Shake_Strength = 10f,
            Shake_Dampening = 0.9f,
        });
    }
}

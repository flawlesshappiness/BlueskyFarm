using Godot;

public partial class KeyItem : Touchable
{
    [Export(PropertyHint.File)]
    public string InfoPath;

    public KeyItemInfo Info => GD.Load<KeyItemInfo>(InfoPath);

    public override void _Ready()
    {
        base._Ready();
        OnTouched += Touched;
    }

    private void Touched()
    {
        KeyItemController.Instance.Add(Info);

        SoundController.Instance.Play("sfx_pickup_key_item", new SoundSettings
        {
            Bus = SoundBus.SFX,
            Volume = 10
        });

        QueueFree();
    }
}

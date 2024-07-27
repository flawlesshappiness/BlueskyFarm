using Godot;

public partial class KeyItem : Touchable
{
    [Export]
    public string Id;

    [Export]
    public string ItemName;

    [Export]
    public Texture2D Icon;

    public override void _Ready()
    {
        base._Ready();
        OnTouched += Touched;
    }

    private void Touched()
    {
        KeyItemInventoryController.Instance.Add(this);

        SoundController.Instance.Play("sfx_pickup_key_item", new SoundSettings
        {
            Bus = SoundBus.SFX,
            Volume = 10
        });

        QueueFree();
    }
}

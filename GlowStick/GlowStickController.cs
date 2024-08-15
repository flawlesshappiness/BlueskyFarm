using Godot;

public partial class GlowStickController : SingletonController
{
    public override string Directory => "GlowStick";
    public static GlowStickController Instance => Singleton.Get<GlowStickController>();
    private FirstPersonController Player => FirstPersonController.Instance;

    public int GlowStickCount => GlowStickData?.Count ?? 0;
    public KeyItemData GlowStickData => KeyItemController.Instance.Get(GlowStickInfo.Id);
    public KeyItemInfo GlowStickInfo => _glowStickInfo ?? (_glowStickInfo = GD.Load<KeyItemInfo>(KeyItemController.Instance.Collection.GlowStick));
    private KeyItemInfo _glowStickInfo;

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);

        if (PlayerInput.GlowStick.Pressed)
        {
            ThrowGlowStick();
        }
    }

    private void ThrowGlowStick()
    {
        if (GlowStickCount <= 0) return;
        KeyItemController.Instance.Remove(GlowStickInfo.Id);

        var rng = new RandomNumberGenerator();
        var rot = 5;
        var speed = 10;
        var item = CreateGlowStick();

        item.GlobalPosition = Player.GlobalPosition.Add(y: 1);
        item.RigidBody.LinearVelocity = Player.Camera.GlobalBasis * (Vector3.Forward + Vector3.Up * 0.25f).Normalized() * speed;
        item.RigidBody.AngularVelocity = new Vector3(rng.RandfRange(-rot, rot), rng.RandfRange(-rot, rot), rng.RandfRange(-rot, rot));

        SoundController.Instance.Play("sfx_throw_glowstick", new SoundSettings
        {
            Bus = SoundBus.SFX,
            Volume = -10,
            PitchMin = 0.9f,
            PitchMax = 1,
        });
    }

    private Item CreateGlowStick()
    {
        var path = ItemController.Instance.Collection.GlowStick;
        var item = ItemController.Instance.CreateItem(path);
        return item;
    }
}

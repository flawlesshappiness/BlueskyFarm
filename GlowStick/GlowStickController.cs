using Godot;
using System;

public partial class GlowStickController : SingletonController
{
    public override string Directory => "GlowStick";
    public static GlowStickController Instance => Singleton.Get<GlowStickController>();
    private FirstPersonController Player => FirstPersonController.Instance;

    public event Action OnGlowSticksChanged;

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);

        if (PlayerInput.GlowStick.Pressed)
        {
            ThrowGlowStick();
        }
    }

    public void AdjustGlowSticks(int count)
    {
        Data.Game.GlowStickCount += count;
        Data.Game.Save();

        OnGlowSticksChanged?.Invoke();
    }

    private void ThrowGlowStick()
    {
        if (Data.Game.GlowStickCount <= 0) return;
        AdjustGlowSticks(-1);

        var rng = new RandomNumberGenerator();
        var rot = 5;
        var speed = 10;
        var item = CreateGlowStick();

        item.GlobalPosition = Player.GlobalPosition.Add(y: 1);
        item.LinearVelocity = Player.Camera.GlobalBasis * (Vector3.Forward + Vector3.Up * 0.25f).Normalized() * speed;
        item.AngularVelocity = new Vector3(rng.RandfRange(-rot, rot), rng.RandfRange(-rot, rot), rng.RandfRange(-rot, rot));
    }

    private Item CreateGlowStick()
    {
        var path = ItemController.Instance.Collection.GlowStick;
        var item = ItemController.Instance.CreateItem(path);
        return item;
    }
}

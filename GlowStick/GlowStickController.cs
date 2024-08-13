using Godot;

public partial class GlowStickController : SingletonController
{
    public override string Directory => "GlowStick";

    private FirstPersonController Player => FirstPersonController.Instance;

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

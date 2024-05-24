using Godot;
using System.Collections;

public partial class FarmBounds : NodeScript
{
    public static FarmBounds Instance { get; private set; }

    [NodeType(typeof(Area3D))]
    public Area3D Bounds;

    [NodeType(typeof(CollisionShape3D))]
    public CollisionShape3D Shape;

    public Vector3 Size => (Shape.Shape as BoxShape3D).Size;
    public Vector3 HalfSize => Size * 0.5f;

    public override void _Ready()
    {
        base._Ready();
        Instance = this;
        Bounds.BodyExited += BodyExited;
    }

    private void BodyExited(Node3D body)
    {
        if (body.IsQueuedForDeletion()) return;
        CallDeferred(nameof(BodyExitedDeferred), body);
    }

    private void BodyExitedDeferred(Node3D body)
    {
        var item = body.GetNodeInChildren<Item>();
        if (item != null && item.Info.CanSell)
        {
            Sell(item);
        }
        else
        {
            ThrowObjectBack(body);
        }
    }

    private void Sell(Item item)
    {
        Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            while (item.GlobalPosition.Y > 0)
            {
                yield return null;
            }

            item.QueueFree();

            ThrowCoins(item.Info.SellValue);
        }
    }

    private void ThrowCoins(int count)
    {
        var position = FirstPersonController.Instance.GlobalPosition;
        for (int i = 0; i < count; i++)
        {
            var coin = ItemController.Instance.SpawnCoin();
            ThrowObject(coin, position);
        }
    }

    private void ThrowObjectBack(Node3D body)
    {
        Debug.LogMethod(body);
        Debug.Indent++;

        var rig = body as RigidBody3D;
        if (rig == null)
        {
            Debug.Indent--;
            return;
        }

        if (rig.Freeze)
        {
            Debug.Indent--;
            return;
        }

        Debug.Indent--;

        Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            while (body.GlobalPosition.Y > 0)
            {
                yield return null;
            }

            ThrowObject(rig, rig.Position);
        }
    }

    public void ThrowObject(RigidBody3D body, Vector3 position)
    {
        Debug.LogMethod($"{body}, {position}");
        Debug.Indent++;

        // Calculate sign
        var center = Bounds.GlobalPosition;
        var direction = position - center;
        var x_is_bigger = Mathf.Abs(direction.X) > Mathf.Abs(direction.Z);
        var x_sign = !x_is_bigger ? 0 : direction.X > 0 ? 1 : -1;
        var z_sign = x_is_bigger ? 0 : direction.Z > 0 ? 1 : -1;

        // Clamp position
        var max = HalfSize * 0.9f;
        var x = !x_is_bigger ? Mathf.Clamp(position.X, -max.X, max.X) : HalfSize.X * x_sign;
        var z = x_is_bigger ? Mathf.Clamp(position.Z, -max.Z, max.Z) : HalfSize.Z * z_sign;
        var y = position.Y;
        position = new Vector3(x, y, z);

        // Calculate position and velocity
        var x_position = position.X + 4f * x_sign;
        var z_position = position.Z + 4f * z_sign;
        var y_position = 2f;
        var throw_position = new Vector3(x_position, y_position, z_position);
        var throw_velocity = new Vector3(-x_sign, 1.5f, -z_sign) * 6f;

        // Throw
        body.GlobalPosition = throw_position;
        body.LinearVelocity = throw_velocity;

        var rng = new RandomNumberGenerator();
        body.AngularVelocity = new Vector3(rng.Randf() * 5, rng.Randf() * 5, rng.Randf() * 5);

        Debug.Indent--;
    }
}

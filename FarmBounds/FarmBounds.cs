using Godot;
using System.Collections;

public partial class FarmBounds : NodeScript
{
    public static FarmBounds Instance { get; private set; }

    [NodeType]
    public Area3D Bounds;

    [NodeType]
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
        CallDeferred(nameof(BodyExitedDeferred), body);
    }

    private void BodyExitedDeferred(GodotObject obj)
    {
        var node = obj as Node3D;
        if (!IsInstanceValid(node)) return;
        if (node.IsQueuedForDeletion()) return;

        var item = node.GetNodeInParents<Item>();

        if (item?.IsBeingHandled ?? false) return;

        ThrowObjectBack(node);
    }

    private void ThrowObjectBack(Node3D body)
    {
        Debug.TraceMethod(body);
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
        Debug.TraceMethod($"{body}, {position}");
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

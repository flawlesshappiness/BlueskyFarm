using Godot;
using System.Collections;

public partial class FarmBounds : NodeScript
{
    [NodeType(typeof(Area3D))]
    public Area3D Bounds;

    [NodeType(typeof(CollisionShape3D))]
    public CollisionShape3D Shape;

    public Vector3 Size => (Shape.Shape as BoxShape3D).Size;

    public override void _Ready()
    {
        base._Ready();
        Bounds.BodyExited += BodyExited;
    }

    private void BodyExited(Node3D body)
    {
        // Default
        ThrowObjectBack(body);
    }

    private Coroutine ThrowObjectBack(Node3D body)
    {
        var rig = body as RigidBody3D;
        var center = Bounds.GlobalPosition;
        var exit_position = body.GlobalPosition;
        var direction = exit_position - center;
        var x_is_bigger = Mathf.Abs(direction.X) > Mathf.Abs(direction.Z);
        var x_sign = !x_is_bigger ? 0 : direction.X > 0 ? 1 : -1;
        var z_sign = x_is_bigger ? 0 : direction.Z > 0 ? 1 : -1;

        var x_position = exit_position.X + 4f * x_sign;
        var z_position = exit_position.Z + 4f * z_sign;
        var y_position = 2f;
        var throw_position = new Vector3(x_position, y_position, z_position);

        var throw_velocity = new Vector3(-x_sign, 1.5f, -z_sign) * 6f;

        return Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            while (body.GlobalPosition.Y > 0)
            {
                yield return null;
            }

            body.GlobalPosition = throw_position;
            rig.LinearVelocity = throw_velocity;
        }
    }
}

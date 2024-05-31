using Godot;
using System.Collections.Generic;

public static class Node3DExtensions
{
    public static void SetEnabled(this Node3D node, bool enabled)
    {
        Rec(node);
        node.Visible = enabled;

        void Rec(Node3D parent)
        {
            if (parent == null) return;

            var shape = parent as CollisionShape3D;
            if (shape != null)
            {
                shape.Disabled = !enabled;
            }

            var children = parent.GetChildren();
            foreach (var child in children)
            {
                Rec(child as Node3D);
            }
        }
    }

    #region RAYCAST
    public static bool TryRaycast(this Node3D node, Vector3 start, Vector3 direction, float length, uint collision_mask, out RaycastResult3D result) => node.TryRaycast(start, start + direction * length, collision_mask, out result);
    public static bool TryRaycast(this Node3D node, Vector3 start, Vector3 end, uint collision_mask, out RaycastResult3D result)
    {
        var space = node.GetWorld3D().DirectSpaceState;
        var query = PhysicsRayQueryParameters3D.Create(start, end, collision_mask);
        var intersection = space.IntersectRay(query);

        if (intersection.Keys.Count > 0)
        {
            intersection.TryGetValue("position", out var position);
            intersection.TryGetValue("normal", out var normal);
            intersection.TryGetValue("collider", out var collider);
            result = new RaycastResult3D
            {
                Position = position.AsVector3(),
                Normal = normal.AsVector3(),
                Collider = collider.AsGodotObject(),
            };
            return true;
        }
        else
        {
            result = null;
            return false;
        }
    }

    public static bool TryRaycast(this Node2D node, Vector2 start, Vector2 direction, float length, uint collision_mask, out RaycastResult2D result) => node.TryRaycast(start, start + direction * length, collision_mask, out result);
    public static bool TryRaycast(this Node2D node, Vector2 start, Vector2 end, uint collision_mask, out RaycastResult2D result)
    {
        var space = node.GetWorld2D().DirectSpaceState;
        var query = PhysicsRayQueryParameters2D.Create(start, end, collision_mask);
        var intersection = space.IntersectRay(query);

        if (intersection.Keys.Count > 0)
        {
            intersection.TryGetValue("position", out var position);
            intersection.TryGetValue("normal", out var normal);
            intersection.TryGetValue("collider", out var collider);
            result = new RaycastResult2D
            {
                Position = position.AsVector2(),
                Normal = normal.AsVector2(),
                Collider = collider.AsGodotObject(),
            };
            return true;
        }
        else
        {
            result = null;
            return false;
        }
    }

    public static IEnumerable<OverlapShapeResult3D> OverlapSphere(this Node3D node, Vector3 origin, float radius, uint collision_mask)
    {
        var shape = new SphereShape3D();
        shape.Radius = radius;
        return node.OverlapShape(shape, origin, collision_mask);
    }

    public static IEnumerable<OverlapShapeResult3D> OverlapShape(this Node3D node, Resource shape, Vector3 origin, uint collision_mask)
    {
        var space = node.GetWorld3D().DirectSpaceState;
        var query = new PhysicsShapeQueryParameters3D();
        query.Transform = query.Transform.Translated(origin);
        query.Shape = shape;
        query.CollisionMask = collision_mask;
        var intersections = space.IntersectShape(query);

        foreach (var intersection in intersections)
        {
            if (intersection.Keys.Count > 0)
            {
                intersection.TryGetValue("collider", out var collider);
                var result = new OverlapShapeResult3D
                {
                    Collider = collider.AsGodotObject(),
                };

                yield return result;
            }
        }
    }
    #endregion
}

public class RaycastResult3D
{
    public Vector3 Position { get; set; }
    public Vector3 Normal { get; set; }
    public GodotObject Collider { get; set; }
}

public class RaycastResult2D
{
    public Vector2 Position { get; set; }
    public Vector2 Normal { get; set; }
    public GodotObject Collider { get; set; }
}

public class OverlapShapeResult3D
{
    public GodotObject Collider { get; set; }
}
using Godot;
using System;
using System.Collections.Generic;

public static partial class NodeExtensions
{
    public static void SetParent(this Node node, Node parent, bool keepGlobalTransform = true)
    {
        if (node.GetParent() == null)
        {
            parent.AddChild(node);
        }
        else
        {
            node.Reparent(parent, keepGlobalTransform);
        }
    }

    public static T GetNodeInChildren<T>(this Node node, string name) where T : Node
    {
        if (node.Name == name) return node as T;

        foreach (var child in node.GetChildren())
        {
            var valid_child = child.GetNodeInChildren<T>(name);
            if (valid_child != null)
                return valid_child;
        }

        return null;
    }

    public static T GetNodeInChildren<T>(this Node node) where T : Node
    {
        if (node.TryGetNode<T>(out var result)) return result;

        foreach (var child in node.GetChildren())
        {
            T script = child.GetNodeInChildren<T>();
            if (script != null)
                return script;
        }

        return null;
    }

    public static Node GetNodeInChildren(this Node node, Type type)
    {
        if (IsSameOrSubclass(type, node.GetType())) return node;

        foreach (var child in node.GetChildren())
        {
            var childNode = child.GetNodeInChildren(type);
            if (childNode != null)
                return childNode;
        }

        return null;
    }

    private static bool IsSameOrSubclass(Type potentialBase, Type potentialDescendant)
    {
        return potentialDescendant.IsSubclassOf(potentialBase) || potentialDescendant == potentialBase;
    }

    public static List<T> GetNodesInChildren<T>(this Node node) where T : Node
    {
        var list = new List<T>();
        Recursive(node);
        return list;

        void Recursive(Node current)
        {
            if (current.TryGetNode<T>(out var result))
            {
                list.Add(result);
            }

            foreach (var child in current.GetChildren())
            {
                Recursive(child);
            }
        }
    }

    public static T GetNodeInParents<T>(this Node node) where T : Node
    {
        var current = node;
        while (current != null)
        {
            if (current.TryGetNode(out T script))
            {
                return script;
            }

            current = current.GetParent();
        }

        return null;
    }

    public static List<T> GetNodesInParents<T>(this Node node) where T : Node
    {
        var list = new List<T>();
        var current = node;
        while (current != null)
        {
            if (current.TryGetNode(out T script))
            {
                list.Add(script);
            }

            current = current.GetParent();
        }

        return list;
    }

    public static bool TryGetNode<T>(this Node parent, out T script) where T : Node
    {
        try
        {
            script = parent.GetNode<T>(parent.GetPath());
            return script != null;
        }
        catch
        {
            script = null;
            return false;
        }
    }

    public static bool TryGetNode<T>(this Node parent, string path, out T script) where T : Node
    {
        try
        {
            script = parent.GetNodeOrNull<T>(path);
            return script != null;
        }
        catch
        {
            script = null;
            return false;
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
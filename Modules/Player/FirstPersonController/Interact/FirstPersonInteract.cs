using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class FirstPersonInteract : RayCast3D, IPlayerInteract
{
    [Export]
    public float SearchRadius;

    public Vector3 RayEndPosition => RayStartPosition + GlobalBasis * TargetPosition;
    public Vector3 RayStartPosition => GlobalPosition;

    public Interactable CurrentInteractable => current_interactable;
    private Interactable current_interactable;

    private Node3D current_collider;

    public override void _Process(double delta)
    {
        base._Process(delta);
        ProcessInteractRaycast();
    }

    private void ProcessInteractRaycast()
    {
        if (IsColliding())
        {
            current_collider = GetCollider() as Node3D;
            var interactable = current_collider.GetNodeInParents<Interactable>(2);
            if (HasLineOfSightTo(interactable))
            {
                SetInteractable(interactable);
            }
        }
        else if (OverlapInteract(out var closest) && HasLineOfSightTo(closest))
        {
            SetInteractable(closest);
        }
        else
        {
            current_collider = null;
            current_interactable = null;
        }
    }

    private bool OverlapInteract(out Interactable closest)
    {
        var valids = new List<Interactable>();
        var hits = this.OverlapSphere(RayEndPosition, SearchRadius, CollisionMask);
        foreach (var hit in hits)
        {
            var node = hit.Collider as Node3D;
            var interactable = node.GetNodeInParents<Interactable>(2);
            if (interactable == null) continue;
            valids.Add(interactable);
        }

        closest = valids
            .OrderBy(x => RayEndPosition.DistanceTo(x.Body.GlobalPosition))
            .FirstOrDefault();

        return valids.Count > 0;
    }

    private bool HasLineOfSightTo(Interactable interactable)
    {
        if (!IsInstanceValid(interactable)) return false;

        var start = RayStartPosition;
        var end = interactable.Body.GlobalPosition;
        var dir = end - start;
        var length = dir.Length() + SearchRadius;
        var cm = CollisionMaskHelper.Create(2, 3);

        if (this.TryRaycast(start, dir, length, cm, out var result))
        {
            var n3 = result.Collider as Node3D;
            var result_interactable = n3.GetNodeInParents<Interactable>();
            return result_interactable == interactable;
        }

        return false;
    }

    private void SetInteractable(Interactable interactable)
    {
        if (interactable == null) return;
        if (current_interactable == interactable) return;
        current_interactable = interactable;
    }
}

using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class FirstPersonInteract : RayCast3D
{
    [Export]
    public float SearchRadius;

    public Vector3 RayEndPosition => RayStartPosition + GlobalBasis * TargetPosition;
    public Vector3 RayStartPosition => GlobalPosition;
    public IInteractable CurrentInteractable { get; private set; }
    public Node3D CurrentCollider { get; private set; }

    public override void _Process(double delta)
    {
        base._Process(delta);
        ProcessCurrentInteractable();
        ProcessInteractRaycast();
    }

    private void ProcessCurrentInteractable()
    {
        if (CurrentInteractable == null) return;

        if (!IsInstanceValid(CurrentInteractable.Body))
        {
            CurrentInteractable = null;
        }
        else if (!CurrentInteractable.Enabled)
        {
            CurrentInteractable = null;
        }
    }


    private void ProcessInteractRaycast()
    {
        if (IsColliding())
        {
            CurrentCollider = GetCollider() as Node3D;
            var interactable = CurrentCollider.GetNodeInParents<IInteractable>(2);
            SetInteractable(interactable);
        }
        else if (OverlapInteract(out var closest) && HasLineOfSightTo(closest))
        {
            SetInteractable(closest);
        }
        else
        {
            CurrentCollider = null;
            CurrentInteractable = null;
        }
    }

    private bool OverlapInteract(out IInteractable closest)
    {
        var valids = new List<IInteractable>();
        var hits = this.OverlapSphere(RayEndPosition, SearchRadius, CollisionMask);
        foreach (var hit in hits)
        {
            var node = hit.Collider as Node3D;
            var interactable = node.GetNodeInParents<IInteractable>(2);
            if (interactable == null) continue;
            valids.Add(interactable);
        }

        closest = valids
            .OrderBy(x => RayEndPosition.DistanceTo(x.Body.GlobalPosition))
            .FirstOrDefault();

        return valids.Count > 0;
    }

    private bool HasLineOfSightTo(IInteractable interactable)
    {
        if (!IsInstanceValid(interactable.Body)) return false;

        var start = RayStartPosition;
        var end = interactable.Body.GlobalPosition;
        var dir = end - start;
        var length = dir.Length() + SearchRadius;
        var mask = CollisionMaskHelper.Create(2, 3);

        if (this.TryRaycast(start, dir, length, mask, out var result))
        {
            var n3 = result.Collider as Node3D;
            var result_interactable = n3.GetNodeInParents<IInteractable>();
            return result_interactable == interactable;
        }

        return false;
    }

    private void SetInteractable(IInteractable interactable)
    {
        if (interactable == null) return;
        if (CurrentInteractable == interactable) return;
        if (!interactable.Enabled) return;
        CurrentInteractable = interactable;
    }
}

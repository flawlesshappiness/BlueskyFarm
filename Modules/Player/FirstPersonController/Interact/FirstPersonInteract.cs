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
            SetInteractable(interactable);
        }
        else if (OverlapInteract(out var closest))
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

    private void SetInteractable(Interactable interactable)
    {
        if (interactable == null) return;
        if (current_interactable == interactable) return;
        current_interactable = interactable;
    }
}

using Godot;
using System.Collections.Generic;

public partial class Enemy : Node3DScript
{
    protected FirstPersonController Player => FirstPersonController.Instance;
    protected Vector3 PlayerPosition => Player.GlobalPosition;
    protected float DistanceToPlayer => GlobalPosition.DistanceTo(PlayerPosition);
    protected Vector3 DirectionToPlayer => GlobalPosition.DirectionTo(PlayerPosition);
    protected IEnumerable<BasementRoomElement> RoomElements => BasementController.Instance.CurrentBasement.Grid.Elements;

    public bool CanSeePlayer()
    {
        if (this.TryRaycast(GlobalPosition.Add(y: 0.25f), PlayerPosition.Add(y: 0.5f), CollisionMaskHelper.Create(CollisionMaskName.Player), out var result))
        {
            var collider = result.Collider as Node;
            var player = collider.GetNodeInParents<FirstPersonController>();
            return IsInstanceValid(player);
        }

        return false;
    }
}

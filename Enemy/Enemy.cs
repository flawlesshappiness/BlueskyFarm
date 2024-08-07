using Godot;
using System.Collections.Generic;

public partial class Enemy : Node3DScript
{
    protected FirstPersonController Player => FirstPersonController.Instance;
    protected Vector3 PlayerPosition => Player.GlobalPosition;
    protected float DistanceToPlayer => GlobalPosition.DistanceTo(PlayerPosition);
    protected Vector3 DirectionToPlayer => GlobalPosition.DirectionTo(PlayerPosition);
    protected IEnumerable<BasementRoomElement> RoomElements => BasementController.Instance.CurrentBasement.Grid.Elements;
}

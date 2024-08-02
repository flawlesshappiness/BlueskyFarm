using Godot;

public partial class Enemy : Node3DScript
{
    protected FirstPersonController Player => FirstPersonController.Instance;
    protected Vector3 PlayerPosition => Player.GlobalPosition;
}

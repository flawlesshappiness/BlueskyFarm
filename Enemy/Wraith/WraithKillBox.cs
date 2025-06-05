using Godot;

public partial class WraithKillBox : Node3D
{
    [Export]
    public Marker3D SpawnMarker;

    [Export]
    public AudioStreamPlayer SfxBreathe;

    public void TeleportPlayer()
    {
        Player.Instance.GlobalPosition = SpawnMarker.GlobalPosition;
        Player.Instance.SetLookRotation(SpawnMarker);
        Player.Instance.Velocity = Vector3.Zero;
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        WraithEnemy.KillBoxRemoved();
    }
}

using Godot;

public partial class OtherFarm : Node3D
{
    [Export]
    public OtherFarmClock Clock;

    [Export]
    public AnimationPlayer AnimationPlayer_SecretRoom;

    [Export]
    public Marker3D SecretRoomItemMarker;

    public override void _Ready()
    {
        base._Ready();
        Clock.OnSolved += ClockSolved;
    }

    private void ClockSolved()
    {
        AnimationPlayer_SecretRoom.Play("open");
    }
}

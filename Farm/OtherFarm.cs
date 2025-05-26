using Godot;

public partial class OtherFarm : Node3D
{
    [Export]
    public OtherFarmClock Clock;

    [Export]
    public AnimationPlayer AnimationPlayer_SecretRoom;

    [Export]
    public Marker3D SecretRoomItemMarker;

    [Export]
    public ItemInfo PotionItem;

    public override void _Ready()
    {
        base._Ready();

        InitializePotionItem();

        Clock.OnSolved += ClockSolved;
    }

    private void InitializePotionItem()
    {
        if (Player.HasAccessToItem(PotionItem)) return;

        var item = ItemController.Instance.CreateItem(PotionItem);
        item.SetParent(SecretRoomItemMarker);
        item.Position = Vector3.Zero;
        item.Rotation = Vector3.Zero;
    }

    private void ClockSolved()
    {
        AnimationPlayer_SecretRoom.Play("open");

        GameFlagIds.SecretPuzzleClockCompleted.SetTrue();
    }
}

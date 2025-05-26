using Godot;
using Godot.Collections;
using System.Linq;

public partial class ForestTombstonePuzzleRoom : Node3D
{
    [Export]
    public Marker3D InventoryItemMarker;

    [Export]
    public Marker3D SecretItemMarker;

    [Export]
    public ItemInfo InventoryItem;

    [Export]
    public ItemInfo PotionItem;

    [Export]
    public CrushableRock Tombstone;

    [Export]
    public AnimationPlayer AnimationPlayer_SecretRoom;

    [Export]
    public Array<LightTrigger> SecretRoomTriggers;

    [Export]
    public Array<LightTrigger> SecretRoomTriggers_Valid;

    private Item _inventory_item;

    public override void _Ready()
    {
        base._Ready();
        InitializeInventoryItem();
        InitializeTombstone();
        InitializeSecretRoom();
        InitializePotionItem();
    }

    private void InitializeInventoryItem()
    {
        if (GameFlagIds.ForestTombstoneInventoryItemPicked.IsTrue()) return;

        var item = ItemController.Instance.CreateItem(InventoryItem);
        item.SetParent(InventoryItemMarker);
        item.Position = Vector3.Zero;
        item.Rotation = Vector3.Zero;
        item.Disable();
        _inventory_item = item;

        item.OnPickUp += () =>
        {
            GameFlagIds.ForestTombstoneInventoryItemPicked.SetTrue();
        };
    }

    private void InitializeTombstone()
    {
        var crushed = GameFlagIds.ForestTombstoneCrushed.IsTrue();
        Tombstone.SetEnabled(!crushed);

        Tombstone.OnCrushed += () =>
        {
            if (_inventory_item != null)
            {
                _inventory_item.Enable();
            }

            Tombstone.SetEnabled(false);
            GameFlagIds.ForestTombstoneCrushed.SetTrue();
        };
    }

    private void InitializeSecretRoom()
    {
        SecretRoomTriggers.ForEach(x => x.OnToggle += ToggleLightTrigger);
    }

    private void InitializePotionItem()
    {
        if (Player.HasAccessToItem(PotionItem)) return;

        var item = ItemController.Instance.CreateItem(PotionItem);
        item.SetParent(SecretItemMarker);
        item.Position = Vector3.Zero;
        item.Rotation = Vector3.Zero;
    }

    private void ToggleLightTrigger(bool toggle)
    {
        var valid = SecretRoomTriggers.All(x => x.IsToggled == SecretRoomTriggers_Valid.Contains(x));

        if (valid)
        {
            AnimationPlayer_SecretRoom.Play("open");
            SecretRoomTriggers.ForEach(x => x.IsToggleable = false);

            GameFlagIds.SecretPuzzleForestCompleted.SetTrue();
        }
    }
}

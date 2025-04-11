using Godot;

public partial class ForestTombstonePuzzleRoom : Node3D
{
    [Export]
    public Marker3D InventoryItemMarker;

    [Export]
    public ItemInfo InventoryItem;

    [Export]
    public CrushableRock Tombstone;

    private Item _inventory_item;

    public override void _Ready()
    {
        base._Ready();
        InitializeInventoryItem();
        InitializeTombstone();
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
}

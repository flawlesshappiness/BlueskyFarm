using Godot;

public partial class ForestFounderHutRoom : Node3DScript
{
    [Export]
    public Weed_Thorns BlockingWeeds;

    [Export]
    public Marker3D InventoryItemMarker;

    [Export]
    public ItemInfo InventoryItem;

    public override void _Ready()
    {
        base._Ready();
        InitializeBlockingWeeds();
        InitializeBlueprintInventory();
    }

    private void InitializeBlockingWeeds()
    {
        BlockingWeeds.SetCut(GameFlagIds.ForestHutWeedsCut.IsTrue());
        BlockingWeeds.OnWeedCut += OnCut;

        void OnCut()
        {
            GameFlagIds.ForestHutWeedsCut.SetTrue();
        }
    }

    private void InitializeBlueprintInventory()
    {
        if (GameFlagIds.ForestHutInventoryItemPicked.IsTrue()) return;

        var item = ItemController.Instance.CreateItem(InventoryItem);
        item.SetParent(InventoryItemMarker);
        item.Position = Vector3.Zero;

        item.OnPickUp += () =>
        {
            GameFlagIds.ForestHutInventoryItemPicked.SetTrue();
        };
    }
}

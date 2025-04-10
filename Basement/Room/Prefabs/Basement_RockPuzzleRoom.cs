using Godot;

public partial class Basement_RockPuzzleRoom : Node3D
{
    [Export]
    public Marker3D InventoryItemMarker;

    [Export]
    public ItemInfo InventoryItem;

    [Export]
    public CrushableRock CrushableRock;

    public override void _Ready()
    {
        base._Ready();
        InitializeInventoryItem();
        InitializeCrushableRock();
    }

    private void InitializeInventoryItem()
    {
        if (GameFlagIds.BasementRockInventoryItemPicked.IsTrue()) return;

        var item = ItemController.Instance.CreateItem(InventoryItem);
        item.SetParent(InventoryItemMarker);
        item.Position = Vector3.Zero;
        item.Rotation = Vector3.Zero;

        item.OnPickUp += () =>
        {
            GameFlagIds.BasementRockInventoryItemPicked.SetTrue();
        };
    }

    private void InitializeCrushableRock()
    {
        var crushed = GameFlagIds.BasementRockCrushed.IsTrue();
        CrushableRock.SetEnabled(!crushed);

        CrushableRock.OnCrushed += () =>
        {
            CrushableRock.SetEnabled(false);
            GameFlagIds.BasementRockCrushed.SetTrue();
        };
    }
}

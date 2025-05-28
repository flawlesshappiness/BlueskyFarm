using Godot;

public partial class ForestFounderHutRoom : Node3DScript
{
    [Export]
    public Weed_Thorns BlockingWeeds;

    [Export]
    public Marker3D InventoryItemMarker;

    [Export]
    public Marker3D PlantBoxBlueprintMarker;

    [Export]
    public ItemInfo InventoryItem;

    [Export]
    public BlueprintInfo PlantBoxBlueprintInfo;

    public override void _Ready()
    {
        base._Ready();
        InitializeBlockingWeeds();
        InitializeItem();
        InitializePlantBoxBlueprint();
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

    private void InitializeItem()
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

    private void InitializePlantBoxBlueprint()
    {
        if (Player.HasAccessToBlueprint(PlantBoxBlueprintInfo.Id)) return;
        if (Player.HasCraftedBlueprint(PlantBoxBlueprintInfo.Id)) return;

        var item = BlueprintController.Instance.CreateBlueprintRoll(PlantBoxBlueprintInfo.Id);
        item.SetParent(PlantBoxBlueprintMarker);
        item.Position = Vector3.Zero;
        item.Rotation = Vector3.Zero;
    }
}

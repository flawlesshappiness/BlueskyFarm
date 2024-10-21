using Godot;

[GlobalClass]
public partial class ItemInfo : Resource
{
    [Export(PropertyHint.File)]
    public string Path { get; set; }

    [Export]
    public ItemType Type { get; set; }

    [Export]
    public bool IsSeed;

    [Export]
    public ItemType SeedResultType;

    [Export]
    public int GrowTimeInSeconds;

    [Export]
    public bool CanProcessToMaterial;

    [Export]
    public CraftingMaterialType MaterialType;

    [Export]
    public bool CanPickUp;

    [Export]
    public bool Untrack;

    [Export]
    public float InventoryItemCameraDistance = 0.25f;

    [Export]
    public float InventoryItemRotationOffset = 0.0f;
}

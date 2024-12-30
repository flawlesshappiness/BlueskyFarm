using Godot;

[GlobalClass]
public partial class ItemInfo : Resource
{
    [Export]
    public PackedScene Scene;

    [Export]
    public string ItemName;

    [Export]
    public string CustomId;

    [Export]
    public ItemType Type;

    [Export]
    public int Uses;

    [Export]
    public ItemType SeedResultType;

    [Export]
    public int GrowTimeInSeconds;

    [Export]
    public bool CanPickUp;

    [Export]
    public bool PerishesOnFarm;

    [Export]
    public bool Untrack;

    [Export]
    public float InventoryItemCameraDistance = 0.25f;

    [Export]
    public float InventoryItemRotationOffset = 0.0f;
}

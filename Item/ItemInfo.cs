using Godot;

[GlobalClass]
public partial class ItemInfo : Resource
{
    [Export(PropertyHint.File)]
    public string Path;

    [Export]
    public string ItemName;

    [Export]
    public ItemType Type;

    [Export]
    public int Uses;

    [Export]
    public bool IsSeed;

    [Export]
    public ItemType SeedResultType;

    [Export]
    public int GrowTimeInSeconds;

    [Export]
    public bool CanPickUp;

    [Export]
    public bool Untrack;

    [Export]
    public float InventoryItemCameraDistance = 0.25f;

    [Export]
    public float InventoryItemRotationOffset = 0.0f;
}

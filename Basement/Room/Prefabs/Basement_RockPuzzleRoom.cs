using Godot;

public partial class Basement_RockPuzzleRoom : Node3D
{
    [Export]
    public Marker3D InventoryItemMarker;

    [Export]
    public Marker3D SecretInventoryItemMarker;

    [Export]
    public ItemInfo InventoryItem;

    [Export]
    public CrushableRock CrushableRock;

    [Export]
    public BasementDoor BlockedDoor;

    [Export]
    public AnimationPlayer AnimationPlayer_Secret;

    [Export]
    public LightTrigger LightTrigger;

    public override void _Ready()
    {
        base._Ready();
        InitializeInventoryItem();
        InitializeCrushableRock();
        InitializeBlockedDoor();
        InitializeSecretInventoryItem();
        InitializeLightTrigger();
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

    private void InitializeSecretInventoryItem()
    {
        if (GameFlagIds.BasementRockSecretInventoryItemPicked.IsTrue()) return;

        var item = ItemController.Instance.CreateItem(InventoryItem);
        item.SetParent(SecretInventoryItemMarker);
        item.Position = Vector3.Zero;
        item.Rotation = Vector3.Zero;

        item.OnPickUp += () =>
        {
            GameFlagIds.BasementRockSecretInventoryItemPicked.SetTrue();
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

    private void InitializeBlockedDoor()
    {
        BlockedDoor.Locked = true;
    }

    private void InitializeLightTrigger()
    {
        LightTrigger.OnToggle += LightTriggerToggle;
    }

    private void LightTriggerToggle(bool toggle)
    {
        if (toggle)
        {
            LightTrigger.IsToggleable = false;
            AnimationPlayer_Secret.Play("open");
        }
    }
}

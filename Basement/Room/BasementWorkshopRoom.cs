using Godot;
using System.Collections;

public partial class BasementWorkshopRoom : Node3DScript
{
    [Export]
    public BasementDoor WorkshopDoor;

    [Export]
    public ItemArea WorkshopDoorItemArea;

    [Export]
    public Marker3D WeedcutterBlueprintMarker;

    [Export]
    public Marker3D PlantBoxBlueprintMarker;

    [Export]
    public BlueprintInfo WeedcutterBlueprintInfo;

    [Export]
    public BlueprintInfo PlantBoxBlueprintInfo;

    public override void _Ready()
    {
        base._Ready();
        WorkshopDoorItemArea.OnItemEntered += WorkshopDoorItemArea_ItemEntered;

        SetWorkshopDoorLocked(GameFlagIds.BasementWorkshopDoorUnlocked.IsFalse());
    }

    protected override void Initialize()
    {
        base.Initialize();
        InitializeWeedcutterBlueprint();
        InitializePlantBoxBlueprint();
    }

    private void WorkshopDoorItemArea_ItemEntered(Item item)
    {
        item.IsBeingHandled = true;
        GameFlagIds.BasementWorkshopDoorUnlocked.SetTrue();

        Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            var position = item.GlobalPosition;
            SoundController.Instance.Play("sfx_throw_light", position);
            yield return item.AnimateDisappearAndQueueFree();
            SoundController.Instance.Play("sfx_unlock", position);
            SoundController.Instance.Play("sfx_puzzle_basement");
            SetWorkshopDoorLocked(false);
        }
    }

    private void SetWorkshopDoorLocked(bool locked)
    {
        WorkshopDoor.Locked = locked;
        WorkshopDoorItemArea.SetEnabled(locked);
    }

    private void InitializeWeedcutterBlueprint()
    {
        if (Player.HasAccessToBlueprint(WeedcutterBlueprintInfo.Id)) return;
        if (Player.HasAccessToItem(WeedcutterBlueprintInfo.ResultItemInfo)) return;

        var item = BlueprintController.Instance.CreateBlueprintRoll(WeedcutterBlueprintInfo.Id);
        item.SetParent(WeedcutterBlueprintMarker);
        item.Position = Vector3.Zero;
        item.Rotation = Vector3.Zero;
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

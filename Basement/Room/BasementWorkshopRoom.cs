using Godot;
using System.Collections;
using System.Linq;

public partial class BasementWorkshopRoom : Node3DScript
{
    [NodeName]
    public BasementDoor WorkshopDoor;

    [NodeName]
    public ItemArea WorkshopDoorItemArea;

    [NodeName]
    public Node3D BlueprintPosition;

    public override void _Ready()
    {
        base._Ready();
        WorkshopDoorItemArea.OnItemEntered += WorkshopDoorItemArea_ItemEntered;

        SetWorkshopDoorLocked(!Data.Game.Flag_WorkshopDoorUnlocked);
    }

    protected override void Initialize()
    {
        base.Initialize();
        CreateBlueprint();
    }

    private void WorkshopDoorItemArea_ItemEntered(Item item)
    {
        item.IsBeingHandled = true;
        Data.Game.Flag_WorkshopDoorUnlocked = true;

        Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            var position = item.GlobalPosition;
            SoundController.Instance.Play("sfx_throw_light", position);
            yield return item.AnimateDisappearAndQueueFree();
            SoundController.Instance.Play("sfx_unlock", position);
            SetWorkshopDoorLocked(false);
        }
    }

    private void SetWorkshopDoorLocked(bool locked)
    {
        WorkshopDoor.Locked = locked;
        WorkshopDoorItemArea.SetEnabled(locked);
    }

    private void CreateBlueprint()
    {
        var bp_id = "weedcutter";
        var item_path = ItemController.Instance.Collection.GetResource("Weedcutter").ResourcePath;
        var scene_data = Data.Game.Scenes.FirstOrDefault(x => x.Name == nameof(FarmScene));
        var bp_in_scene = scene_data.Items.Any(x => x.Blueprint?.Id == bp_id);
        var item_in_scene = scene_data.Items.Any(x => x.Info == item_path);
        var bp_in_inv = Data.Game.InventoryItems.Any(x => x != null && x.Blueprint?.Id == bp_id);
        var item_in_inv = Data.Game.InventoryItems.Any(x => x != null && x.Info == item_path);
        var cancel = bp_in_scene || bp_in_inv || item_in_scene || item_in_inv;

        if (cancel) return;

        var item = BlueprintController.Instance.CreateBlueprintRoll(bp_id);
        item.GlobalPosition = BlueprintPosition.GlobalPosition;
    }
}

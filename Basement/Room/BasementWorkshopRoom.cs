using Godot;
using System.Collections;

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
        Data.Game.Flag_WorkshopKeyAvailable = false;

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
        var item_id = "Weedcutter";
        var has_bp = Player.Instance.HasAccessToBlueprint(bp_id);
        var has_item = Player.Instance.HasAccessToItem(item_id);
        var cancel = has_bp || has_item;

        if (cancel) return;

        var item = BlueprintController.Instance.CreateBlueprintRoll(bp_id);
        item.GlobalPosition = BlueprintPosition.GlobalPosition;
    }
}

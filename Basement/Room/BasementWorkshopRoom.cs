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
        DialogueController.Instance.SetFlag(DialogueFlags.FrogWeedcutter, 3);

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

    private void CreateBlueprint()
    {
        var bp_id = "weedcutter";
        var item_id = "Weedcutter";

        if (Player.HasAccessToBlueprint(bp_id)) return;
        if (Player.HasAccessToItem(item_id)) return;

        var item = BlueprintController.Instance.CreateBlueprintRoll(bp_id);
        item.SetParent(BlueprintPosition);
        item.Position = Vector3.Zero;
    }
}

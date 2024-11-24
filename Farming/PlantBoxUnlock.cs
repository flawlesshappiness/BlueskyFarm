using Godot;
using System.Collections;

public partial class PlantBoxUnlock : Node3DScript
{
    [NodeType]
    public PlantArea PlantArea;

    [NodeName]
    public Area3D AreaUnlock;

    public const string ITEM_UNLOCK_ID = "plant_box_unlock";

    public override void _Ready()
    {
        base._Ready();

        AreaUnlock.BodyEntered += go => CallDeferred(nameof(BodyEntered), go);

        LoadData();
    }

    private void LoadData()
    {
        var is_unlocked = Data.Game.UnlockedPlantBoxes.Contains(PlantArea.Id);
        PlantArea.SetEnabled(is_unlocked);
        AreaUnlock.SetEnabled(!is_unlocked);
    }

    public void Unlock(Item item)
    {
        if (!Data.Game.UnlockedPlantBoxes.Contains(PlantArea.Id))
        {
            Data.Game.UnlockedPlantBoxes.Add(PlantArea.Id);
        }

        StartCoroutine(Cr, "unlock");
        IEnumerator Cr()
        {
            SoundController.Instance.Play("sfx_throw_light", item.GlobalPosition);
            yield return item.AnimateDisappearAndQueueFree();
            PlantArea.Enable();
            SoundController.Instance.Play("sfx_throw_light", PlantArea.GlobalPosition);
            yield return PlantArea.AnimateAppear();
        }
    }

    private void BodyEntered(GodotObject go)
    {
        var node = go as Node3D;
        if (!IsInstanceValid(node)) return;

        var item = node.GetNodeInParents<Item>();
        if (!IsInstanceValid(item)) return;

        if (item.Data.CustomId == ITEM_UNLOCK_ID)
        {
            Unlock(item);
        }
    }
}
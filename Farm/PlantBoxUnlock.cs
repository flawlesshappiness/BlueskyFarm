using Godot;
using System.Collections;

public partial class PlantBoxUnlock : Node3DScript
{
    [Export]
    public PlantArea PlantArea;

    [Export]
    public Node3D Broken;

    [Export]
    public Area3D AreaUnlock;

    public const string ITEM_UNLOCK_ID = "plant_box_unlock";

    public override void _Ready()
    {
        base._Ready();

        AreaUnlock.BodyEntered += go => CallDeferred(nameof(BodyEntered), go);

        var rng = new RandomNumberGenerator();
        var r = rng.RandfRange(0, 360);
        Broken.GlobalRotationDegrees = new Vector3(0, r, 0);

        LoadData();
    }

    private void LoadData()
    {
        var is_unlocked = Data.Game.UnlockedPlantBoxes.Contains(PlantArea.Id);
        PlantArea.SetEnabled(is_unlocked);
        Broken.SetEnabled(!is_unlocked);
        AreaUnlock.SetEnabled(!is_unlocked);
    }

    public void Unlock(Item item)
    {
        if (!Data.Game.UnlockedPlantBoxes.Contains(PlantArea.Id))
        {
            Data.Game.UnlockedPlantBoxes.Add(PlantArea.Id);
        }

        this.StartCoroutine(Cr, "unlock");
        IEnumerator Cr()
        {
            SoundController.Instance.Play("sfx_throw_light", item.GlobalPosition);
            yield return item.AnimateDisappearAndQueueFree();
            PlantArea.Enable();
            Broken.Disable();
            Particle.PlayOneShot("ps_dirt_puff", PlantArea.GlobalPosition);
            SoundController.Instance.Play("sfx_plant_box_create", PlantArea.GlobalPosition);
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
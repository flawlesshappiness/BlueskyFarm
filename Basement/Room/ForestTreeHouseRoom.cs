using Godot;
using Godot.Collections;
using System.Linq;

public partial class ForestTreeHouseRoom : Node3D
{
    [Export]
    public Touchable MissingStepsTouchable;

    [Export]
    public Node3D BlueprintItemSpawn;

    [Export]
    public ItemInfo PlankInfo;

    [Export]
    public LadderArea LadderArea;

    [Export]
    public Array<Node3D> PlankSpawns;

    [Export]
    public Array<ForestTreeHousePlankPosition> PlankPositions;

    public override void _Ready()
    {
        base._Ready();
        CreateBlueprint();
        CreatePlanks();
        InitializePlankPositions();

        MissingStepsTouchable.OnTouched += MissingSteps_Touched;
    }

    private void CreatePlanks()
    {
        if (GameFlagIds.ForestTreeHouseRepaired.IsTrue()) return;

        foreach (var node in PlankSpawns)
        {
            var item = ItemController.Instance.CreateItem(PlankInfo);
            item.Freeze = true;
            item.SetParent(node);
            item.Position = Vector3.Zero;
            item.RotationDegrees = Vector3.Zero;

            item.OnGrabbed += () =>
            {
                item.Freeze = false;
            };
        }
    }

    private void CreateBlueprint()
    {
        var bp_id = "bed_repair";
        if (Player.HasAccessToBlueprint(bp_id)) return;
        if (GameFlagIds.FarmBedRepaired.IsTrue()) return;

        var item = BlueprintController.Instance.CreateBlueprintRoll(bp_id);
        item.SetParent(BlueprintItemSpawn);
        item.Position = Vector3.Zero;
    }

    private void InitializePlankPositions()
    {
        LadderArea.SetEnabled(GameFlagIds.ForestTreeHouseRepaired.IsTrue());
        MissingStepsTouchable.SetEnabled(GameFlagIds.ForestTreeHouseRepaired.IsFalse());

        foreach (var position in PlankPositions)
        {
            position.SetRepaired(GameFlagIds.ForestTreeHouseRepaired.IsTrue());
            position.OnRepaired += PlankRepaired;
        }
    }

    private void PlankRepaired()
    {
        if (PlankPositions.All(x => x.Repaired))
        {
            GameFlagIds.ForestTreeHouseRepaired.SetTrue();
            MissingStepsTouchable.Disable();
            LadderArea.Enable();
        }
    }

    private void MissingSteps_Touched()
    {
        SoundController.Instance.Play("sfx_throw_light");
    }
}

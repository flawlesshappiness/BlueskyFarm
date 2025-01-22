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
        foreach (var node in PlankSpawns)
        {
            var item = ItemController.Instance.CreateItem(PlankInfo);
            item.LockPosition_All();
            item.LockRotation_All();
            item.SetParent(node);
            item.Position = Vector3.Zero;
            item.RotationDegrees = Vector3.Zero;

            item.OnGrabbed += () =>
            {
                item.UnlockPosition_All();
                item.UnlockRotation_All();
            };
        }
    }

    private void CreateBlueprint()
    {
        var bp_id = "bed_repair";
        var has_bp = Player.Instance.HasAccessToBlueprint(bp_id);
        var has_item = Data.Game.Flag_BedRepaired;
        var cancel = has_bp || has_item;

        if (cancel) return;

        var item = BlueprintController.Instance.CreateBlueprintRoll(bp_id);
        item.SetParent(BlueprintItemSpawn);
        item.Position = Vector3.Zero;
    }

    private void InitializePlankPositions()
    {
        LadderArea.SetEnabled(Data.Game.Flag_ForestTreeHouseRepaired);
        MissingStepsTouchable.SetEnabled(!Data.Game.Flag_ForestTreeHouseRepaired);

        foreach (var position in PlankPositions)
        {
            position.SetRepaired(Data.Game.Flag_ForestTreeHouseRepaired);
            position.OnRepaired += PlankRepaired;
        }
    }

    private void PlankRepaired()
    {
        if (PlankPositions.All(x => x.Repaired))
        {
            Data.Game.Flag_ForestTreeHouseRepaired = true;
            MissingStepsTouchable.Disable();
            LadderArea.Enable();
        }
    }

    private void MissingSteps_Touched()
    {
        SoundController.Instance.Play("sfx_throw_light");

        GameView.Instance.CreateText(new CreateTextSettings
        {
            Id = "missing_steps_" + GetInstanceId(),
            Text = "##LADDER_MISSING_STEPS##",
            Target = MissingStepsTouchable,
            Offset = new Vector3(0, 0.2f, 0),
            Duration = 5.0f,
        });
    }
}

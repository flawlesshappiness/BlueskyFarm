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
        if (Data.Game.Flag_ForestTreeHouseRepaired) return;

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
        if (Data.Game.Flag_BedRepaired) return;

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
            Duration = 3.0f,
        });
    }
}

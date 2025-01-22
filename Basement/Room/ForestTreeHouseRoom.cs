using Godot;

public partial class ForestTreeHouseRoom : Node3D
{
    [Export]
    public Node3D BlueprintItemSpawn;

    public override void _Ready()
    {
        base._Ready();
        CreateBlueprint();
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
}

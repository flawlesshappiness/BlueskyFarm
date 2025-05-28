using Godot;

public partial class MineBedsRoom : Node3D
{
    [Export]
    public Marker3D PickaxeBlueprintMarker;

    [Export]
    public Marker3D PlantBoxBlueprintMarker;

    [Export]
    public BlueprintInfo PickaxeBlueprintInfo;

    [Export]
    public BlueprintInfo PlantBoxBlueprintInfo;

    public override void _Ready()
    {
        base._Ready();
        InitializeBlueprint();
        InitializePlantBoxBlueprint();
    }

    private void InitializeBlueprint()
    {
        if (Player.HasAccessToBlueprint(PickaxeBlueprintInfo.Id)) return;
        if (Player.HasAccessToItem(PickaxeBlueprintInfo.ResultItemInfo)) return;

        var item = BlueprintController.Instance.CreateBlueprintRoll(PickaxeBlueprintInfo.Id);
        item.SetParent(PickaxeBlueprintMarker);
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

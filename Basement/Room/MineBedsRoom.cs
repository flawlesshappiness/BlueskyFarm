using Godot;

public partial class MineBedsRoom : Node3D
{
    [Export]
    public Marker3D PickaxeBlueprintMarker;

    [Export]
    public BlueprintInfo PickaxeBlueprintInfo;

    public override void _Ready()
    {
        base._Ready();
        InitializeBlueprint();
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
}

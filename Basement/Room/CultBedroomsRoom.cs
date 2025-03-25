using Godot;

public partial class CultBedroomsRoom : Node3D
{
    [Export]
    public Marker3D MoldBlueprintMarker;

    [Export]
    public BlueprintInfo MoldBlueprint;

    public override void _Ready()
    {
        base._Ready();
        InitializeBlueprint();
    }

    private void InitializeBlueprint()
    {
        if (Player.HasAccessToBlueprint(MoldBlueprint.Id)) return;
        if (Player.HasAccessToItem(MoldBlueprint.ResultItemInfo)) return;

        var item = BlueprintController.Instance.CreateBlueprintRoll(MoldBlueprint.Id);
        item.SetParent(MoldBlueprintMarker);
        item.Position = Vector3.Zero;
        item.Rotation = Vector3.Zero;
    }
}

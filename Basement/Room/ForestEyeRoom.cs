using Godot;

public partial class ForestEyeRoom : Node3D
{
    [Export]
    public AnimationPlayer PlatformAnimationPlayer;

    [Export]
    public LightTrigger PlatformLightTrigger;

    [Export]
    public Marker3D BlueprintMarker;

    [Export]
    public BlueprintInfo BlueprintInfo;

    public override void _Ready()
    {
        base._Ready();

        InitializeBlueprint();

        PlatformLightTrigger.OnToggle += LightTriggerToggle;
    }

    private void LightTriggerToggle(bool toggle)
    {
        if (toggle)
        {
            PlatformLightTrigger.IsToggleable = false;
            PlatformAnimationPlayer.Play("move_up");
        }
    }

    private void InitializeBlueprint()
    {
        if (Player.HasAccessToBlueprint(BlueprintInfo.Id)) return;
        if (Player.HasAccessToItem(BlueprintInfo.ResultItemInfo)) return;

        var item = BlueprintController.Instance.CreateBlueprintRoll(BlueprintInfo.Id);
        item.SetParent(BlueprintMarker);
        item.Position = Vector3.Zero;
        item.Rotation = Vector3.Zero;
    }
}

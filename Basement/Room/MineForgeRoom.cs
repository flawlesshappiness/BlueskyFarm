using Godot;

public partial class MineForgeRoom : Node3D
{
    [Export]
    public BasementRoom Room;

    [Export]
    public Node3D PickaxeMarker;

    [Export]
    public ItemInfo PickaxeInfo;

    public override void _Ready()
    {
        base._Ready();
        RegisterDebugActions();
        CreatePickaxeItem();
    }

    private void RegisterDebugActions()
    {
        var category = "BASEMENT";

        Debug.RegisterAction(new DebugAction
        {
            Category = category,
            Text = "Teleport to forge",
            Action = _ => TeleportToForge()
        });

        void TeleportToForge()
        {
            Player.Instance.GlobalPosition = Room.EnemyMarker.GlobalPosition;
        }
    }

    private void CreatePickaxeItem()
    {
        if (Player.HasAccessToItem(PickaxeInfo)) return;

        var item = ItemController.Instance.CreateItem(PickaxeInfo);
        item.SetParent(PickaxeMarker);
        item.Transform = Transform3D.Identity;
    }
}

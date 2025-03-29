using Godot;

public partial class MineForgeRoom : Node3D
{
    [Export]
    public BasementRoom Room;

    [Export]
    public ForgeKiln Kiln;

    [Export]
    public ForgeMachine Forge;

    [Export]
    public Node3D PickaxeMarker;

    [Export]
    public ItemInfo PickaxeInfo;

    private string DebugId => GetInstanceId().ToString();

    public override void _Ready()
    {
        base._Ready();
        RegisterDebugActions();
        CreatePickaxeItem();
        InitializeKiln();
    }

    private void InitializeKiln()
    {
        Kiln.SetActivated(Data.Game.Flag_MineKilnActivated);
        Forge.SetActivated(Data.Game.Flag_MineKilnActivated);
        Kiln.OnActivated += KilnActivated;
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        Debug.RemoveActions(DebugId);
    }

    private void RegisterDebugActions()
    {
        var category = "BASEMENT - FORGE";

        Debug.RegisterAction(new DebugAction
        {
            Id = DebugId,
            Category = category,
            Text = "Teleport to",
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

    private void KilnActivated()
    {
        Data.Game.Flag_MineKilnActivated = true;
        Forge.SetActivated(true);
    }
}

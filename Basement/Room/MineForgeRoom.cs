using Godot;

public partial class MineForgeRoom : Node3D
{
    [Export]
    public Node3D PickaxeMarker;

    [Export]
    public ItemInfo PickaxeInfo;

    public override void _Ready()
    {
        base._Ready();
        CreatePickaxeItem();
    }

    private void CreatePickaxeItem()
    {
        if (Player.HasAccessToItem(PickaxeInfo)) return;

        var item = ItemController.Instance.CreateItem(PickaxeInfo);
        item.SetParent(PickaxeMarker);
        item.Transform = Transform3D.Identity;
    }
}

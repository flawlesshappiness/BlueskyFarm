using Godot;
using Godot.Collections;
using System.Linq;

public partial class MineCoalRoom : Node3D
{
    [Export]
    public ItemInfo CoalInfo;

    [Export]
    public Array<Marker3D> CoalItemPositions;

    private Label _debug_info_label;

    public override void _Ready()
    {
        base._Ready();
        CreateCoalItems();
    }

    private void CreateCoalItems()
    {
        var markers = CoalItemPositions.ToList();

        var count = 6;
        for (int i = 0; i < count; i++)
        {
            var item = ItemController.Instance.CreateItem(CoalInfo);
            var marker = markers.PopRandom();
            item.SetParent(marker);
            item.ClearPositionAndRotation();
        }
    }
}

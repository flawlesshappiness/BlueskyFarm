using Godot;

public partial class ItemSpawn_MaterialProcessorOil : Node3DScript
{
    [Export(PropertyHint.File)]
    public string ItemInfoPath;

    protected override void Initialize()
    {
        base.Initialize();
        InitializeItem();
    }

    private void InitializeItem()
    {
        if (!Data.Game.Flag_MaterialProcessorFixed)
        {
            CreateItem();
        }
    }

    private Item CreateItem()
    {
        var item = ItemController.Instance.CreateItem(ItemInfoPath);
        item.GlobalPosition = GlobalPosition;
        return item;
    }
}

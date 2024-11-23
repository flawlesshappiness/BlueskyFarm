using Godot;

public partial class ItemSpawn_MaterialProcessorOil : Node3DScript
{
    [Export(PropertyHint.File)]
    public string ItemInfoPath;

    [Export]
    public string CustomId;

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
        var item = ItemController.Instance.CreateItemFromPath(ItemInfoPath);
        item.GlobalPosition = GlobalPosition;
        item.Data.CustomId = CustomId;
        return item;
    }
}

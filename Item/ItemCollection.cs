using Godot;

[GlobalClass]
public partial class ItemCollection : ResourceCollection<ItemInfo>
{
    [Export]
    public ItemInfo Seed;

    [Export]
    public ItemInfo Blueprint;
}

using Godot;

[GlobalClass]
public partial class ItemCollection : ResourceCollection<ItemInfo>
{
    [Export(PropertyHint.File)]
    public string Seed;

    [Export(PropertyHint.File)]
    public string Coin;
}

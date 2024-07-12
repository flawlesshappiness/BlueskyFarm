using Godot;

[GlobalClass]
public partial class ShopItemInfo : Resource
{
    [Export(PropertyHint.File)]
    public string ItemInfoPath;

    [Export]
    public string Combination;

    [Export]
    public int Price;
}

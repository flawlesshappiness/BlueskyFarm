using Godot;
using Godot.Collections;

[GlobalClass]
public partial class ShopItemInfo : Resource
{
    [Export(PropertyHint.File)]
    public string ItemInfoPath;

    [Export]
    public Array<CraftingMaterialType> Combination;
}

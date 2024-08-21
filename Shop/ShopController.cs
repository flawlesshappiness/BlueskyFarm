using System.Collections.Generic;
using System.Linq;

public partial class ShopController : ResourceController<ShopItemInfoCollection, ShopItemInfo>
{
    public static ShopController Instance => Singleton.Get<ShopController>();
    public override string Directory => "Shop";

    public ShopItemInfo GetShopItem(List<CraftingMaterialType> combination)
    {
        return Collection.Resources.FirstOrDefault(x => x.Combination.SequenceEqual(combination));
    }

    public Item CreateShopItem(ShopItemInfo info)
    {
        return ItemController.Instance.CreateItem(info.ItemInfoPath);
    }
}

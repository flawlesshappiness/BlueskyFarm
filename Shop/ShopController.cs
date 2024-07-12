public partial class ShopController : ResourceController<ShopItemInfoCollection, ShopItemInfo>
{
    public static ShopController Instance => Singleton.Get<ShopController>();
    public override string Directory => "Shop";

    public ShopItemInfo GetShopItem(string combination)
    {
        return Collection.Resources.Find(x => x.Combination == combination);
    }

    public bool CanAfford(ShopItemInfo info)
    {
        return CurrencyController.Instance.GetValue(CurrencyType.Money) >= info.Price;
    }

    public void PurchaseShopItem(ShopItemInfo info)
    {
        CurrencyController.Instance.AddValue(CurrencyType.Money, -info.Price);
    }

    public Item CreateShopItem(ShopItemInfo info)
    {
        return ItemController.Instance.CreateItem(info.ItemInfoPath);
    }
}

using System.Linq;

public partial class SeedController : ResourceController<SeedCollection, SeedInfo>
{
    public static SeedController Instance => Singleton.Get<SeedController>();
    public override string Directory => "Seed";

    public Item CreateSeed(ItemType type)
    {
        var seed_info = Collection.Resources.FirstOrDefault(x => x.ItemType == type);
        if (seed_info == null) return null;

        var result = ItemController.Instance.Collection.Resources
            .Where(x => x.Type == type)
            .ToList().Random();

        var item = ItemController.Instance.CreateItem(seed_info.ItemInfo);
        item.Data.Seed = new SeedData
        {
            Info = result.ResourcePath,
        };

        return item;
    }
}
using System.Collections.Generic;
using System.Linq;

public partial class SeedController : SingletonController
{
    public static SeedController Instance => Singleton.Get<SeedController>();
    public override string Directory => "Farming";

    private List<ItemType> _valid_types = new()
    {
        ItemType.Vegetable
    };

    public Item CreateSeed(ItemType type)
    {
        var valid = _valid_types.Contains(type);
        if (!valid) return null;

        var result = ItemController.Instance.Collection.Resources
            .Where(x => x.Type == type)
            .ToList().Random();

        var seed_name = GetSeedName(type);
        var item = ItemController.Instance.CreateItem(seed_name);
        item.Data.Seed = new SeedData
        {
            Info = result.ResourcePath,
        };

        return item;
    }

    private string GetSeedName(ItemType type) => type switch
    {
        ItemType.Vegetable => "Seed_Plant",
        _ => ""
    };
}

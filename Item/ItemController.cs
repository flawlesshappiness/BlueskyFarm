using Godot;
using System.IO;

public partial class ItemController : ResourceController<ItemCollection, ItemInfo>
{
    public static ItemController Instance => Singleton.Get<ItemController>();
    public override string Directory => "Item";

    public override void _Ready()
    {
        base._Ready();
        RegisterDebugActions();
    }

    public Item CreateItem(ItemInfo info)
    {
        var item = GDHelper.Instantiate<Item>(info.Path);
        item.Info = info;
        return item;
    }

    public Item CreateItem(string info_path)
    {
        var info = GD.Load<ItemInfo>(info_path);
        return CreateItem(info);
    }

    public Item SpawnCoin()
    {
        var item = CreateItem(Collection.Coin);
        item.OnGrabbed += PickCoin;
        return item;

        void PickCoin()
        {
            CurrencyController.Instance.AddValue(CurrencyType.Money, 1);
            Particle.PlayOneShot(ParticleType.SmokePuffSmall, item.Node.GlobalPosition);
            item.Node.QueueFree();
        }
    }

    public void SellItem(ItemInfo info)
    {
        Debug.LogMethod($"{info}");
        Debug.Indent++;

        CurrencyController.Instance.AddValue(CurrencyType.Money, info.SellValue);
        Data.Game.Save();

        Debug.Indent--;
    }

    private void RegisterDebugActions()
    {
        var category = "ITEMS";

        Debug.RegisterAction(new DebugAction
        {
            Category = category,
            Text = "Spawn item",
            Action = DebugSpawnItem
        });

        Debug.RegisterAction(new DebugAction
        {
            Category = category,
            Text = "Spawn seed",
            Action = DebugSpawnSeed
        });
    }

    private void DebugSpawnItem(DebugView view)
    {
        view.HideContent();
        view.Content.Show();
        view.ContentSearch.Show();

        view.ContentSearch.ClearItems();
        foreach (var resource in Collection.Resources)
        {
            view.ContentSearch.AddItem(Path.GetFileName(resource.ResourcePath), () => SelectItem(resource));
        }

        view.ContentSearch.UpdateButtons();

        void SelectItem(ItemInfo info)
        {
            var item = CreateItem(info);
            FarmBounds.Instance.ThrowObject(item, FirstPersonController.Instance.GlobalPosition);
        }
    }

    private void DebugSpawnSeed(DebugView view)
    {
        view.HideContent();
        view.Content.Show();
        view.ContentSearch.Show();

        view.ContentSearch.ClearItems();
        foreach (var resource in Collection.Resources)
        {
            view.ContentSearch.AddItem(Path.GetFileName(resource.ResourcePath), () => SelectItem(resource.ResourcePath));
        }

        view.ContentSearch.UpdateButtons();

        void SelectItem(string path)
        {
            var item = CreateItem(Collection.Seed);
            item.PlantInfo = GD.Load<ItemInfo>(path);
            FarmBounds.Instance.ThrowObject(item, FirstPersonController.Instance.GlobalPosition);
        }
    }
}

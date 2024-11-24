using Godot;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public partial class ItemController : ResourceController<ItemCollection, ItemInfo>
{
    public static ItemController Instance => Singleton.Get<ItemController>();
    public override string Directory => "Item";

    private List<Item> ActiveItems { get; set; } = new();
    private List<ItemData> ActiveItemDatas => ActiveItems.Select(item => item.Data).ToList();

    public override void _Ready()
    {
        base._Ready();
        RegisterDebugActions();
        Scene.OnSceneLoaded += SceneLoaded;
    }

    /// <summary>
    /// Removes item from list of active items. Item won't be persisted on save.
    /// </summary>
    public void UntrackItem(Item item)
    {
        if (ActiveItems.Contains(item))
        {
            ActiveItems.Remove(item);
        }
    }

    public void TrackItem(Item item)
    {
        if (!IsInstanceValid(item)) return;
        if (!ActiveItems.Contains(item))
        {
            ActiveItems.Add(item);
        }
    }

    public void UpdateData()
    {
        ActiveItems.ForEach(item => item.UpdateData());
        Scene.Current.SceneData.Items = ActiveItemDatas;
    }

    private void SceneLoaded()
    {
        ActiveItems.Clear();
        LoadItems(Scene.Current.SceneData.Items);
    }

    public void LoadItems(List<ItemData> datas)
    {
        Debug.TraceMethod();
        Debug.Indent++;

        foreach (var data in datas)
        {
            CreateItemFromData(data);
        }

        Debug.Indent--;
    }

    public ItemData CreateItemData(ItemInfo info)
    {
        return new ItemData
        {
            Info = info.ResourcePath,
            CustomId = info.CustomId
        };
    }

    public Item CreateItemFromData(ItemData data)
    {
        var info = GD.Load<ItemInfo>(data.Info);
        var item = GDHelper.Instantiate<Item>(info.Path);

        item.Info = info;
        item.Data = data;
        item.SetParent(Scene.Current);
        item.LoadFromData();
        ActiveItems.Add(item);
        return item;
    }

    public Item CreateItemFromPath(string info_path, CreateItemSettings settings = null)
    {
        var info = GetInfoFromPath(info_path);
        return CreateItem(info, settings);
    }

    public Item CreateItem(string name, CreateItemSettings settings = null)
    {
        var info = Collection.GetResource(name);
        return CreateItem(info, settings);
    }

    public Item CreateItem(ItemInfo info, CreateItemSettings settings = null)
    {
        var parent = settings?.Parent ?? Scene.Current as Node;
        var item = GDHelper.Instantiate<Item>(info.Path, parent);
        item.Info = info;
        item.Data = CreateItemData(info);

        var track = !info.Untrack && (settings?.Tracked ?? true);
        if (track)
        {
            ActiveItems.Add(item);
        }

        return item;
    }

    public ItemInfo GetInfoFromPath(string path)
    {
        return GD.Load<ItemInfo>(path);
    }

    private void RegisterDebugActions()
    {
        var category = "ITEMS";

        Debug.RegisterAction(new DebugAction
        {
            Category = category,
            Text = "Give item",
            Action = GiveItem
        });

        Debug.RegisterAction(new DebugAction
        {
            Category = category,
            Text = "Give seed",
            Action = GiveSeed
        });

        void GiveSeed(DebugView view)
        {
            view.SetContent_Search();
            view.ContentSearch.AddItem(ItemType.Vegetable.ToString(), () => SelectItemType(ItemType.Vegetable));
            view.ContentSearch.UpdateButtons();

            void SelectItemType(ItemType type)
            {
                var info = Collection.GetResource("Seed_Plant");
                var data = CreateItemData(info);
                var plant_infos = Collection.Resources.Where(info => info.Type == type);
                data.Seed = new SeedData
                {
                    Info = plant_infos.ToList().Random().ResourcePath
                };

                InventoryController.Instance.Add(data);
            }
        }

        void GiveItem(DebugView view)
        {
            view.SetContent_Search();
            foreach (var resource in Collection.Resources)
            {
                view.ContentSearch.AddItem(Path.GetFileName(resource.ResourcePath), () => SelectItem(resource));
            }

            view.ContentSearch.UpdateButtons();

            void SelectItem(ItemInfo info)
            {
                InventoryController.Instance.Add(info);
            }
        }
    }

    private void WriteDebugSpawnItem_CustomIdPopup(DebugView view, ItemInfo info)
    {
        view.PopupStringInput("Custom id", id =>
        {
            SelectItem(info, id);
        });

        void SelectItem(ItemInfo info, string custom_id)
        {
            var item = CreateItem(info);
            item.Data.CustomId = custom_id;
            FarmBounds.Instance.ThrowObject(item.RigidBody, Player.Instance.GlobalPosition);
        }
    }
}

public class CreateItemSettings
{
    public bool Tracked { get; set; } = true;
    public Node3D Parent { get; set; }
}
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

    public Item CreateItemFromData(ItemData data)
    {
        var info = GD.Load<ItemInfo>(data.InfoPath);
        var item = GDHelper.Instantiate<Item>(info.Path);

        item.Info = info;
        item.Data = data;
        item.SetParent(Scene.Current);
        item.LoadFromData();
        ActiveItems.Add(item);
        return item;
    }

    public Item CreateItem(ItemInfo info, bool track_item = true, Node3D parent = null)
    {
        var item = GDHelper.Instantiate<Item>(info.Path, parent ?? Scene.Current as Node);
        item.Info = info;
        item.Data = new ItemData
        {
            InfoPath = info.ResourcePath
        };

        var track = !info.Untrack && track_item;
        if (track)
        {
            ActiveItems.Add(item);
        }

        return item;
    }

    public Item CreateItem(string info_path, bool track_item = true)
    {
        var info = GetInfo(info_path);
        return CreateItem(info, track_item);
    }

    public ItemInfo GetInfo(string path)
    {
        return GD.Load<ItemInfo>(path);
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
            Action = SpawnSeed
        });

        void SpawnSeed(DebugView view)
        {
            view.HideContent();
            view.Content.Show();
            view.ContentSearch.Show();
            view.ContentSearch.ClearItems();

            view.ContentSearch.AddItem(ItemType.Vegetable.ToString(), () => SelectItemType(ItemType.Vegetable));
            view.ContentSearch.UpdateButtons();

            void SelectItemType(ItemType type)
            {
                var infos = Collection.Resources.Where(info => info.Type == type);
                var item = CreateItem(Collection.Seed);
                item.Data.PlantInfoPath = infos.ToList().Random().ResourcePath;
                FarmBounds.Instance.ThrowObject(item.RigidBody, FirstPersonController.Instance.GlobalPosition);
            }
        }
    }

    private void DebugSpawnItem(DebugView view)
    {
        view.HideContent();
        view.Content.Show();
        view.ContentSearch.Show();

        view.ContentSearch.ClearItems();
        foreach (var resource in Collection.Resources)
        {
            view.ContentSearch.AddItem(Path.GetFileName(resource.ResourcePath), () => WritDebugSpawnItem_CustomIdPopup(view, resource));
        }

        view.ContentSearch.UpdateButtons();
    }

    private void WritDebugSpawnItem_CustomIdPopup(DebugView view, ItemInfo info)
    {
        view.PopupStringInput("Custom id", id =>
        {
            SelectItem(info, id);
        });

        void SelectItem(ItemInfo info, string custom_id)
        {
            var item = CreateItem(info);
            item.Data.CustomId = custom_id;
            FarmBounds.Instance.ThrowObject(item.RigidBody, FirstPersonController.Instance.GlobalPosition);
        }
    }
}

using Godot;
using System.IO;
using System.Linq;

public partial class ItemController : ResourceController<ItemCollection, ItemInfo>
{
    public static ItemController Instance => GetController<ItemController>("Item");
    public ItemCollection Collection => GetCollection("Item/Resources/ItemCollection.tres");

    public override void _Ready()
    {
        base._Ready();
        RegisterDebugActions();
    }

    public Node3D CreateItem(string path)
    {
        var item = GDHelper.Instantiate<Node3D>(path);
        return item;
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
        view.Content.Show();
        view.ContentSearch.Show();

        view.ContentSearch.ClearItems();
        foreach (var resource in Collection.Resources)
        {
            view.ContentSearch.AddItem(Path.GetFileName(resource.ResourcePath), () => SelectItem(resource.Path));
        }

        view.ContentSearch.UpdateButtons();

        void SelectItem(string path)
        {
            var item = CreateItem(path);
            FarmBounds.Instance.ThrowObject(item as RigidBody3D, FirstPersonController.Instance.GlobalPosition);
        }
    }

    private void DebugSpawnSeed(DebugView view)
    {
        view.Content.Show();
        view.ContentSearch.Show();

        view.ContentSearch.ClearItems();
        foreach (var resource in Collection.Resources)
        {
            view.ContentSearch.AddItem(Path.GetFileName(resource.ResourcePath), () => SelectItem(resource.Path));
        }

        view.ContentSearch.UpdateButtons();

        void SelectItem(string path)
        {
            var item = CreateItem(Collection.Resources.FirstOrDefault(x => x.Path.Contains("seed", System.StringComparison.OrdinalIgnoreCase)).Path);
            var seed = item.GetNodeInChildren<Seed>();
            seed.PlantPath = path;
            FarmBounds.Instance.ThrowObject(item as RigidBody3D, FirstPersonController.Instance.GlobalPosition);
        }
    }
}

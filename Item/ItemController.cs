using Godot;
using System.IO;

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
        var node = GDHelper.Instantiate<Node3D>(path);
        return node;
    }

    private void RegisterDebugActions()
    {
        var category = "ITEMS";
        Debug.RegisterAction(new DebugAction
        {
            Category = category,
            Text = "Test",
            Action = DebugCreateAndThrowItem
        });
    }

    private void DebugCreateAndThrowItem(DebugView view)
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
            var item = CreateItem(path) as RigidBody3D;
            FarmBounds.Instance.ThrowObject(item, FirstPersonController.Instance.GlobalPosition);
        }
    }
}

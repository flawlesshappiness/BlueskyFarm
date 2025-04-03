using Godot;

public partial class ItemContainer : Node3DScript
{
    public bool HasItem => _item != null;

    protected Item _item;

    protected void SpawnItem(Vector3 position, Vector3 velocity)
    {
        if (!IsInstanceValid(_item)) return;

        _item.Freeze = false;
        _item.SetEnabled(true);
        _item.GlobalPosition = position;
        _item.LinearVelocity = velocity;
    }

    public void SetItem(Item item)
    {
        _item = item;
        _item.Disable();
        _item.Freeze = true;
    }

    public void SetItem(ItemInfo info)
    {
        var item = ItemController.Instance.CreateItem(info);
        SetItem(item);
    }
}

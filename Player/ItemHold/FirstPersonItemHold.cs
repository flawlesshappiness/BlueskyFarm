using Godot;

public partial class FirstPersonItemHold : Node3DScript
{
    [NodeName]
    public new Node3D Position;

    private Item _current_item;

    public override void _Ready()
    {
        base._Ready();
        InventoryController.Instance.OnSelectedItemUsed += SelectedItemUsed;
        InventoryController.Instance.OnSelectedItemChanged += SelectedItemChanged;
        InventoryController.Instance.OnItemAdded += ItemAdded;
        InventoryController.Instance.OnItemRemoved += ItemRemoved;
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        InventoryController.Instance.OnSelectedItemUsed -= SelectedItemUsed;
        InventoryController.Instance.OnSelectedItemChanged -= SelectedItemChanged;
        InventoryController.Instance.OnItemAdded -= ItemAdded;
        InventoryController.Instance.OnItemRemoved -= ItemRemoved;
    }

    private void ItemAdded(int i)
    {
        UpdateHeldItem();
    }

    private void ItemRemoved(int i)
    {
        UpdateHeldItem();
    }

    private void SelectedItemChanged(int i)
    {
        UpdateHeldItem(i);
    }

    private void UpdateHeldItem()
    {
        UpdateHeldItem(InventoryController.Instance.SelectedIndex);
    }

    private void UpdateHeldItem(int i)
    {
        RemoveHeldItem();

        var data = InventoryController.Instance.GetItem(i);
        if (data == null) return;

        _current_item = ItemController.Instance.CreateItemFromData(data);
        ItemController.Instance.UntrackItem(_current_item);

        if (IsInstanceValid(_current_item))
        {
            _current_item.Freeze = true;
            _current_item.SetParent(Position);
            _current_item.ClearCollisionLayerAndMask();
            _current_item.GlobalPosition = Position.GlobalPosition;
            _current_item.GlobalRotation = Position.GlobalRotation;
        }
    }

    private void RemoveHeldItem()
    {
        if (!IsInstanceValid(_current_item)) return;

        _current_item.QueueFree();
        _current_item = null;
    }

    private void SelectedItemUsed(int i)
    {
        if (!IsInstanceValid(_current_item)) return;

        _current_item.Use();
    }
}

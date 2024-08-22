using Godot;

public partial class FirstPersonItemHold : Node3DScript
{
    [NodeName]
    public Node3D Position;

    private Item _current_item;

    public override void _Ready()
    {
        base._Ready();
        InventoryController.Instance.OnSelectedItemChanged += SelectedItemChanged;
        InventoryController.Instance.OnItemAdded += ItemAdded;
        InventoryController.Instance.OnItemRemoved += ItemRemoved;
    }

    public override void _ExitTree()
    {
        base._ExitTree();
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

        _current_item = ItemController.Instance.CreateItem(data.InfoPath, false);

        if (IsInstanceValid(_current_item))
        {
            _current_item.RigidBody.ProcessMode = ProcessModeEnum.Disabled;
            _current_item.SetParent(Position);
            _current_item.SetCollision_None();
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
}

using Godot;
using System.Linq;

public partial class InventoryBar : DynamicUI
{
    [NodeType]
    public InventoryBarElement ElementTemplate;

    private InventoryBarElement[] _elements = new InventoryBarElement[InventoryController.MAX_INVENTORY_SIZE];

    public override void _Ready()
    {
        base._Ready();
        ElementTemplate.Hide();
        InitializeElements();
        InventoryController.Instance.OnItemAdded += ItemAdded;
        InventoryController.Instance.OnItemRemoved += ItemRemoved;
        InventoryController.Instance.OnSelectedItemChanged += SelectedItemChanged;
    }

    private void InitializeElements()
    {
        for (int i = 0; i < _elements.Length; i++)
        {
            var e = CreateElement();
            _elements[i] = e;

            var selected_index = InventoryController.Instance.SelectedIndex;
            if (i == selected_index) e.Select();
            else e.Deselect();

            var data = Data.Game.InventoryItems[i];
            if (data == null) continue;
            var info = GD.Load<ItemInfo>(data.InfoPath);
            if (info == null) continue;
            e.WorldObject.LoadItem(info);
        }

        UpdateElementVisibility();
    }

    private void UpdateElementVisibility()
    {
        var size = Data.Game.InventorySize;
        for (int i = 0; i < _elements.Length; i++)
        {
            _elements[i].Visible = i < size;
        }

        AnimateShow();
    }

    private void ItemRemoved(int i)
    {
        var e = GetElement(i);
        e.Clear();

        AnimateShow();
    }

    private void ItemAdded(int i)
    {
        var data = InventoryController.Instance.GetItem(i);
        if (data == null) return;

        var info = GD.Load<ItemInfo>(data.InfoPath);
        if (info == null) return;

        var e = GetElement(i);
        e.WorldObject.LoadItem(info);

        AnimateShow();
    }

    private void SelectedItemChanged(int i)
    {
        DeselectAllElements();
        var e = GetElement(i);
        e.Select();

        AnimateShow();
    }

    private void DeselectAllElements()
    {
        _elements.ToList().ForEach(x => x.Deselect());
    }

    private InventoryBarElement GetElement(int i)
    {
        return _elements[Mathf.Clamp(i, 0, _elements.Length - 1)];
    }

    private InventoryBarElement CreateElement()
    {
        var e = ElementTemplate.Duplicate() as InventoryBarElement;
        e.SetParent(ElementTemplate.GetParent());
        e.Clear();
        e.Show();
        e.WorldObject.SubViewportContainer.Stretch = true;
        return e;
    }
}

using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class KeyItemBar : ControlScript
{
    [NodeName]
    public TextureRect IconTemplate;

    private List<ItemIcon> _icons = new();

    private class ItemIcon
    {
        public string Id { get; set; }
        public TextureRect Element { get; set; }
    }

    public override void _Ready()
    {
        base._Ready();
        IconTemplate.Hide();

        KeyItemInventoryController.Instance.OnItemAdded += ItemAdded;
        KeyItemInventoryController.Instance.OnItemRemoved += ItemRemoved;
    }

    private void ItemRemoved(string id)
    {
        RemoveIcon(id);
    }

    private void ItemAdded(KeyItem item)
    {
        CreateIcon(item.Icon, item.Id);
    }

    private void CreateIcon(Texture2D icon, string id)
    {
        var element = IconTemplate.Duplicate() as TextureRect;
        element.SetParent(IconTemplate.GetParent());
        element.Show();

        var i = new ItemIcon
        {
            Id = id,
            Element = element
        };

        _icons.Add(i);
    }

    private void RemoveIcon(string id)
    {
        var i = _icons.FirstOrDefault(x => x.Id == id);
        if (i == null) return;

        _icons.Remove(i);
        i.Element.QueueFree();
    }
}

using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class KeyItemBar : ControlScript
{
    [NodeType]
    public KeyItemElement ElementTemplate;

    private List<KeyItemElement> _elements = new();

    public override void _Ready()
    {
        base._Ready();
        ElementTemplate.Hide();

        KeyItemController.Instance.OnItemAdded += ItemAdded;
        KeyItemController.Instance.OnItemRemoved += ItemRemoved;
    }

    private void ItemRemoved(string id)
    {
        var e = GetElement(id);
        if (e == null) return;

        var data = KeyItemController.Instance.Get(id);
        if (data != null)
        {
            e.SetCount(data.Count);
        }
        else
        {
            RemoveElement(id);
        }
    }

    private void ItemAdded(KeyItemInfo info)
    {
        var e = GetElement(info.Id) ?? CreateElement(info.Icon, info.Id);
        var data = KeyItemController.Instance.Get(info.Id);
        e.SetCount(data.Count);
    }

    private KeyItemElement GetElement(string id)
    {
        return _elements.FirstOrDefault(x => x.Id == id);
    }

    private KeyItemElement CreateElement(Texture2D icon, string id)
    {
        var element = ElementTemplate.Duplicate() as KeyItemElement;
        element.SetParent(ElementTemplate.GetParent());
        element.Show();
        element.Id = id;
        element.Icon = icon;
        _elements.Add(element);
        return element;
    }

    private void RemoveElement(string id)
    {
        var e = _elements.FirstOrDefault(x => x.Id == id);
        if (e == null) return;
        RemoveElement(e);
    }

    private void RemoveElement(KeyItemElement e)
    {
        _elements.Remove(e);
        e.QueueFree();
    }
}

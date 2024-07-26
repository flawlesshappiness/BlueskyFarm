using System;
using System.Collections.Generic;
using System.Linq;

public partial class KeyItemInventoryController : SingletonController
{
    public static KeyItemInventoryController Instance => Singleton.Get<KeyItemInventoryController>();
    public override string Directory => "Inventory";

    public event Action<KeyItem> OnItemAdded;
    public event Action<string> OnItemRemoved;

    private List<string> _item_ids = new();

    public bool HasItem(string id) => _item_ids.Contains(id);

    public void Add(KeyItem item)
    {
        Debug.TraceMethod(item.Id);
        Debug.Indent++;

        _item_ids.Add(item.Id);
        OnItemAdded?.Invoke(item);

        Debug.Indent--;
    }

    public void Remove(string id)
    {
        Debug.TraceMethod(id);
        Debug.Indent++;

        _item_ids.Remove(id);
        OnItemRemoved?.Invoke(id);

        Debug.Indent--;
    }

    public void Clear()
    {
        Debug.TraceMethod();

        foreach (var id in _item_ids.ToList())
        {
            Remove(id);
        }

        _item_ids.Clear();
    }
}

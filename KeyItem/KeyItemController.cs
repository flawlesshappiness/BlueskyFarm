using System;
using System.Linq;

public partial class KeyItemController : ResourceController<KeyItemCollection, KeyItemInfo>
{
    public static KeyItemController Instance => Singleton.Get<KeyItemController>();
    public override string Directory => "KeyItem";

    public event Action<KeyItemInfo> OnItemAdded;
    public event Action<string> OnItemRemoved;

    public bool HasItem(string id) => Get(id) != null;

    public KeyItemData Get(string id) => Data.Game.KeyItems.FirstOrDefault(x => x.Id == id);

    public void Add(KeyItemInfo info)
    {
        Debug.TraceMethod(info.Id);
        Debug.Indent++;

        var data = Get(info.Id);
        if (data == null)
        {
            data = new KeyItemData
            {
                Id = info.Id,
                Temporary = info.Temporary
            };
        }

        data.Count += info.Count;

        OnItemAdded?.Invoke(info);

        Debug.Indent--;
    }

    public void Remove(string id, int count = 1)
    {
        Debug.TraceMethod(id);
        Debug.Indent++;

        var data = Get(id);
        if (data != null)
        {
            data.Count -= count;
            if (data.Count <= 0)
            {
                Data.Game.KeyItems.Remove(data);
            }

            OnItemRemoved?.Invoke(id);
        }

        Debug.Indent--;
    }

    public void ClearTemporary()
    {
        Debug.TraceMethod();

        foreach (var data in Data.Game.KeyItems.ToList())
        {
            if (data.Temporary)
            {
                Data.Game.KeyItems.Remove(data);
            }
        }
    }
}

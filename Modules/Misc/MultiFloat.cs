using Godot.Collections;
using System.Linq;

public abstract class MultiFloat
{
    protected Dictionary<string, float> _values = new();

    public abstract float Value { get; }

    public void Clear()
    {
        _values.Clear();
    }

    public void Set(string id, float value)
    {
        if (_values.ContainsKey(id))
        {
            _values[id] = value;
        }
        else
        {
            _values.Add(id, value);
        }
    }

    public void Remove(string id)
    {
        if (_values.ContainsKey(id))
        {
            _values.Remove(id);
        }
    }
}

public class MultiFloatMin : MultiFloat
{
    public override float Value => _values.Values.OrderByDescending(x => x).FirstOrDefault();
}

public class MultiFloatMax : MultiFloat
{
    public override float Value => _values.Values.OrderBy(x => x).FirstOrDefault();
}
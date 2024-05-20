using Godot;
using System.Linq;

public partial class CurrencyController : Node
{
    public static CurrencyController Instance => Singleton.Create<CurrencyController>($"{Paths.Modules}/Currency/{nameof(CurrencyController)}");

    public override void _Ready()
    {
        base._Ready();
        RegisterDebugActions();
    }

    public CurrencyData GetData(CurrencyType type)
    {
        var data = Data.Game.Currencies.FirstOrDefault(x => x.Type.Equals(type));
        if (data == null)
        {
            data = new CurrencyData { Type = type };
            Data.Game.Currencies.Add(data);
        }

        return data;
    }

    public int GetValue(CurrencyType type)
    {
        return GetData(type).Value;
    }

    public void AddValue(CurrencyType type, int value)
    {
        Debug.LogMethod($"{type}, {value}");
        Debug.Indent++;

        var data = GetData(type);
        SetValue(type, data.Value + value);

        Debug.Indent--;
    }

    public void SetValue(CurrencyType type, int value)
    {
        Debug.LogMethod($"{type}, {value}");
        Debug.Indent++;

        var data = GetData(type);
        data.Value = Mathf.Clamp(value, 0, int.MaxValue);

        data.OnValueChanged?.Invoke(data.Value);

        Debug.Indent--;
    }

    private void RegisterDebugActions()
    {
        var category = "Currency";

        Debug.RegisterAction(new DebugAction
        {
            Category = category,
            Text = "Clear ALL currency data",
            Action = DebugClearAllCurrencyData
        });
    }

    private void DebugClearAllCurrencyData(DebugView view)
    {
        Data.Game.Currencies.Clear();
        Data.Game.Save();
        Scene.Tree.Quit();
    }
}

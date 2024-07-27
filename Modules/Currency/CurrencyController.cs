using Godot;
using System.Linq;

public partial class CurrencyController : Node
{
    public static CurrencyController Instance => Singleton.GetOrCreate<CurrencyController>($"{Paths.Modules}/Currency/{nameof(CurrencyController)}");

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
        Debug.TraceMethod($"{type}, {value}");
        Debug.Indent++;

        var data = GetData(type);
        SetValue(type, data.Value + value);

        Debug.Indent--;
    }

    public void SetValue(CurrencyType type, int value)
    {
        Debug.TraceMethod($"{type}, {value}");
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

        Debug.RegisterAction(new DebugAction
        {
            Category = category,
            Text = "Adjust currency",
            Action = DebugAdjustCurrency
        });
    }

    private void DebugClearAllCurrencyData(DebugView view)
    {
        Data.Game.Currencies.Clear();
        Data.Game.Save();
        Scene.Tree.Quit();
    }

    private void DebugAdjustCurrency(DebugView view)
    {
        view.HideContent();
        view.Content.Show();
        view.ContentSearch.Show();
        view.ContentSearch.ClearItems();

        foreach (var currency in Data.Game.Currencies)
        {
            view.ContentSearch.AddItem(currency.Type.Id, () => DebugAdjustCurrency(view, currency.Type));
        }

        view.ContentSearch.UpdateButtons();
    }

    private void DebugAdjustCurrency(DebugView view, CurrencyType type)
    {
        view.HideContent();
        view.Content.Show();
        view.ContentSearch.Show();
        view.ContentSearch.ClearItems();

        view.ContentSearch.AddItem("-100", () => AddValue(type, -100));
        view.ContentSearch.AddItem("-10", () => AddValue(type, -10));
        view.ContentSearch.AddItem("-5", () => AddValue(type, -5));
        view.ContentSearch.AddItem("-1", () => AddValue(type, -1));
        view.ContentSearch.AddItem("+1", () => AddValue(type, 1));
        view.ContentSearch.AddItem("+5", () => AddValue(type, 5));
        view.ContentSearch.AddItem("+10", () => AddValue(type, 10));
        view.ContentSearch.AddItem("+100", () => AddValue(type, 100));

        view.ContentSearch.UpdateButtons();
    }
}

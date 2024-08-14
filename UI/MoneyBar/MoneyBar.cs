using Godot;

public partial class MoneyBar : ControlScript
{
    [NodeName(nameof(MoneyLabel))]
    public Label MoneyLabel;

    [NodeType]
    public DynamicUI DynamicUI;

    public override void _Ready()
    {
        base._Ready();

        var data = CurrencyController.Instance.GetData(CurrencyType.Money);
        data.OnValueChanged += OnMoneyChanged;
        OnMoneyChanged(data.Value);
    }

    private void OnMoneyChanged(int value)
    {
        MoneyLabel.Text = value.ToString();
        DynamicUI.AnimateShow(true);
    }
}

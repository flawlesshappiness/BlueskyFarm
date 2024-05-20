using Godot;

public partial class GameView : View
{
    [NodeName(nameof(MoneyLabel))]
    public Label MoneyLabel;

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
    }
}

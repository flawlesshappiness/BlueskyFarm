using Godot;

public partial class CombinationPrice : Node3DScript
{
    [NodeName]
    public Label3D PriceLabel;

    public bool IsShowingPrice { get; set; }

    public void SetPrice(int price)
    {
        PriceLabel.Text = price.ToString();
        IsShowingPrice = true;
    }

    public void Clear()
    {
        PriceLabel.Text = string.Empty;
        IsShowingPrice = false;
    }
}

using Godot;

[GlobalClass]
public partial class ItemInfo : Resource
{
    [Export(PropertyHint.File)]
    public string Path { get; set; }

    [Export]
    public bool CanSell;

    [Export]
    public int SellValue;

    [Export]
    public bool CanPlant;

    [Export]
    public int GrowTimeInSeconds;

    [Export]
    public bool IsMoney;

    [Export]
    public int MoneyAmount;

    [Export]
    public bool CanPickUp;

    [Export]
    public bool Untrack;
}

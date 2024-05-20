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
}

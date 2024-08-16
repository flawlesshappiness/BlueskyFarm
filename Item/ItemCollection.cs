using Godot;

[GlobalClass]
public partial class ItemCollection : ResourceCollection<ItemInfo>
{
    [Export(PropertyHint.File)]
    public string PlantPot;

    [Export(PropertyHint.File)]
    public string Seed;

    [Export(PropertyHint.File)]
    public string Coin;

    [Export(PropertyHint.File)]
    public string Radish;

    [Export(PropertyHint.File)]
    public string GlowStick;

    [Export(PropertyHint.File)]
    public string Key_Brown;
}

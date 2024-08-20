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

    [Export(PropertyHint.File)]
    public string PlantMaterial;

    [Export(PropertyHint.File)]
    public string MetalMaterial;

    [Export(PropertyHint.File)]
    public string WoodMaterial;

    [Export(PropertyHint.File)]
    public string GlowMaterial;

    [Export(PropertyHint.File)]
    public string RockMaterial;

    [Export(PropertyHint.File)]
    public string CrystalMaterial;
}

using Godot;

[GlobalClass]
public partial class BlueprintInfo : Resource
{
    [Export]
    public string Id;

    [Export]
    public Resource ResultItemInfo;

    [Export]
    public int VegetableCount;
}

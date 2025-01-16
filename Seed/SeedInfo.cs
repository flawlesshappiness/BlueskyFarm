using Godot;

[GlobalClass]
public partial class SeedInfo : Resource
{
    [Export]
    public ItemType ItemType;

    [Export]
    public ItemInfo ItemInfo;
}

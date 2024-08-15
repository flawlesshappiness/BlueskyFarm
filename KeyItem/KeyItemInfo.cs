using Godot;

[GlobalClass]
public partial class KeyItemInfo : Resource
{
    [Export]
    public string Id;

    [Export]
    public string ItemName;

    [Export]
    public int Count = 1;

    [Export]
    public Texture2D Icon;

    [Export]
    public bool Temporary;
}

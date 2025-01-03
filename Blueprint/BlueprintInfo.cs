using Godot;

[GlobalClass]
public partial class BlueprintInfo : Resource
{
    [Export]
    public string Id;

    [Export]
    public Texture2D ResultIcon;

    [Export]
    public ItemInfo ResultItemInfo;

    [Export]
    public string ResultDialogueNode;

    [Export]
    public int VegetableCount;

    [Export]
    public bool CanCancel;
}

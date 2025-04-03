using Godot;
using Godot.Collections;

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
    public string OverrideResultName;

    [Export]
    public string ResultDialogueNode;

    [Export]
    public int VegetableCount;

    [Export]
    public int BoneCount;

    [Export]
    public int StoneCount;

    [Export]
    public bool CanCancel;

    [Export]
    public Array<AreaNameType> Areas;
}

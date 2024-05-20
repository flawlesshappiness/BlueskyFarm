using Godot;

[GlobalClass]
public partial class ItemInfo : Resource
{
    [Export(PropertyHint.File)]
    public string Path { get; set; }
}

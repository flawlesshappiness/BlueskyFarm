using Godot;

[GlobalClass]
public partial class KeyItemCollection : ResourceCollection<KeyItemInfo>
{
    [Export(PropertyHint.File)]
    public string GlowStick;
}

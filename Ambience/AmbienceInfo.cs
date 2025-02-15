using Godot;
using Godot.Collections;

[GlobalClass]
public partial class AmbienceInfo : Resource
{
    [Export]
    public AreaNameType Area;

    [Export]
    public SoundInfo BackgroundSound;

    [Export]
    public Array<SoundInfo> Noises;

    [Export(PropertyHint.Range, "0,1")]
    public float ReverbAmount;
}

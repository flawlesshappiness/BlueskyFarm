using Godot;
using Godot.Collections;

[GlobalClass]
public partial class SolidMaterialInfo : Resource
{
    [Export]
    public SolidMaterialType Type;

    [Export]
    public float StepBaseVolume;

    [Export]
    public Array<AudioStream> WalkSounds;

    [Export]
    public Array<AudioStream> RunSounds;
}

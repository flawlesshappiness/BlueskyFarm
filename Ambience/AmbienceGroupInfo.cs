using Godot;
using Godot.Collections;

[GlobalClass]
public partial class AmbienceGroupInfo : Resource
{
    [Export]
    public string Name;

    [Export]
    public float DistanceMin = 12;

    [Export]
    public float DistanceMax = 20;

    [Export]
    public float VolumeMin = 0;

    [Export]
    public float VolumeMax = 0;

    [Export]
    public float PitchMin = 0.9f;

    [Export]
    public float PitchMax = 1.1f;

    [Export]
    public float DelayMin = 10f;

    [Export]
    public float DelayMax = 15f;

    [Export]
    public Array<AudioStream> Sounds;
}

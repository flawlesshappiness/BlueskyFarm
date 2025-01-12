using Godot;
using Godot.Collections;

[GlobalClass]
public partial class SoundInfo : Resource
{
    [Export]
    public Array<AudioStream> AudioStreams;

    [Export]
    public float Volume = 0f;

    [Export]
    public bool Looping;

    [Export]
    public Vector2 PitchRange = new Vector2(1, 1);

    [Export]
    public SoundBus Bus = SoundBus.SFX;

    [Export]
    public SoundDistance Distance = SoundDistance.Default;

    [Export]
    public SoundAttenuation Attenuation = SoundAttenuation.Default;
}

public enum SoundDistance
{
    Default, Near, Far
}

public enum SoundAttenuation
{
    Default, Disabled
}

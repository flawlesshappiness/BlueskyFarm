using Godot;

public partial class Sound : AudioStreamPlayer
{
    [Export]
    public float PitchMin = 1;

    [Export]
    public float PitchMax = 1;

    public void Play()
    {
        if (Stream == null)
        {
            Debug.LogError(Name);
        }

        RandomizePitch();
        base.Play();
    }

    private void RandomizePitch()
    {
        var rng = new RandomNumberGenerator();
        var pitch = rng.RandfRange(PitchMin, PitchMax);
        PitchScale = pitch;
    }
}

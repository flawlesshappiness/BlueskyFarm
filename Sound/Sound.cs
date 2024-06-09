using Godot;

public partial class Sound : AudioStreamPlayer
{
    [Export]
    public SoundName SoundName;

    [Export]
    public float PitchMin = 1;

    [Export]
    public float PitchMax = 1;

    public override void _Ready()
    {
        base._Ready();
        LoadSound();
    }

    private void LoadSound()
    {
        var info = SoundController.Instance.GetInfo(SoundName);
        if (info == null) return;

        Stream = GD.Load<AudioStream>(info.Path);
    }

    public void Play()
    {
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

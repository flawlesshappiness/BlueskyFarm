using Godot;

[GlobalClass]
public partial class Sound : Node
{
    [Export]
    public SoundInfo SoundInfo;

    private AudioStreamPlayer _asp;

    public void Play()
    {
        _asp = SoundController.Instance.Play(SoundInfo);
    }

    public void Stop()
    {
        if (!IsInstanceValid(_asp)) return;
        _asp.Stop();
    }
}

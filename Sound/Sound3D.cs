using Godot;

[GlobalClass]
public partial class Sound3D : Node3D
{
    [Export]
    public SoundInfo SoundInfo;

    private AudioStreamPlayer3D _asp;

    public void Play()
    {
        Stop();
        _asp = SoundController.Instance.Play(SoundInfo, GlobalPosition);
    }

    public void Stop()
    {
        if (!IsInstanceValid(_asp)) return;
        _asp.Stop();
    }
}

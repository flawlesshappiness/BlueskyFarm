using Godot;

public partial class Sound3D : AudioStreamPlayer3D
{
    [Export]
    public SoundName SoundName;

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
}

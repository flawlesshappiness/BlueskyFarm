using Godot;
using System.Collections;
using System.Linq;

public partial class SoundController : ResourceController<SoundCollection, SoundInfo>
{
    public override string Directory => "Sound";
    public static SoundController Instance => Singleton.Get<SoundController>();

    public SoundInfo GetInfo(SoundName name) => Collection.Resources.FirstOrDefault(x => x.SoundName == name);

    public void Play(SoundName name, SoundSettings settings = null)
    {
        var info = GetInfo(name);
        var sound = new AudioStreamPlayer();
        sound.SetParent(Scene.Root);

        sound.Stream = GD.Load<AudioStream>(info.Path);
        settings?.ModifySound(sound);
        sound.Play();

        Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            var duration = sound.Stream.GetLength();
            yield return new WaitForSeconds(duration);
            sound.QueueFree();
        }
    }
}

public abstract class SoundSettingsBase
{
    protected RandomNumberGenerator RNG { get; private set; } = new RandomNumberGenerator();
    public float PitchMin { get; set; } = 1;
    public float PitchMax { get; set; } = 1;
    public float RandomPitch => RNG.RandfRange(PitchMin, PitchMax);
}

public class SoundSettings : SoundSettingsBase
{
    public void ModifySound(AudioStreamPlayer sound)
    {
        sound.PitchScale = RandomPitch;
    }
}

public class SoundSettings3D : SoundSettingsBase
{
    public Vector3 Position { get; set; }

    public void ModifySound(AudioStreamPlayer3D sound)
    {
        sound.GlobalPosition = Position;
        sound.PitchScale = RandomPitch;
    }
}

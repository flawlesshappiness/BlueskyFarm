using Godot;
using System;
using System.Collections;
using System.Linq;

public partial class SoundController : ResourceController<SoundCollection, SoundInfo>
{
    public override string Directory => "Sound";
    public static SoundController Instance => Singleton.Get<SoundController>();

    public SoundInfo GetInfo(SoundName name) => Collection.Resources.FirstOrDefault(x => x.SoundName == name);

    public void Play(SoundName name, SoundSettings settings = null)
    {
        var sound = new AudioStreamPlayer();
        sound.SetParent(Scene.Root);

        var info = GetInfo(name);
        if (info == null) return;

        sound.Stream = GD.Load<AudioStream>(info.Path);
        settings?.ModifySound(sound);
        sound.Play();

        var duration = Convert.ToSingle(sound.Stream.GetLength());
        DestroyDelay(sound, duration);
    }

    public void Play(SoundName name, SoundSettings3D settings = null)
    {
        var sound = new AudioStreamPlayer3D();
        sound.SetParent(Scene.Root);
        sound.Bus = settings.Bus.ToString();

        var info = GetInfo(name);
        sound.Stream = GD.Load<AudioStream>(info.Path);
        settings?.ModifySound(sound);
        sound.Play();

        var duration = Convert.ToSingle(sound.Stream.GetLength());
        DestroyDelay(sound, duration);
    }

    private void DestroyDelay(Node sound, float delay)
    {
        Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            yield return new WaitForSeconds(delay);
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
    public float Volume { get; set; }
    public SoundBus Bus { get; set; } = SoundBus.Master;
}

public class SoundSettings : SoundSettingsBase
{
    public void ModifySound(AudioStreamPlayer sound)
    {
        sound.Bus = Bus.ToString();
        sound.PitchScale = RandomPitch;
        sound.VolumeDb = Volume;
    }
}

public class SoundSettings3D : SoundSettingsBase
{
    public Vector3 Position { get; set; }

    public void ModifySound(AudioStreamPlayer3D sound)
    {
        sound.GlobalPosition = Position;
        sound.PitchScale = RandomPitch;
        sound.VolumeDb = Volume;
    }
}

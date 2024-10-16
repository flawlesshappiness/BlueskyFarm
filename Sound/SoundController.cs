using Godot;
using System;
using System.Collections;

public partial class SoundController : ResourceController<SoundCollection, SoundInfo>
{
    public override string Directory => "Sound";
    public static SoundController Instance => Singleton.Get<SoundController>();

    public void Play(string name, SoundSettings settings = null) => Play(Collection.GetSound(name), settings);
    public void Play(string name, SoundSettings3D settings = null) => Play(Collection.GetSound(name), settings);

    public void Play(AudioStream stream, SoundSettings3D settings = null)
    {
        if (stream == null) return;

        var sound = new AudioStreamPlayer3D();
        sound.SetParent(Scene.Root);
        sound.Stream = stream;
        settings?.ModifySound(sound);
        sound.Play();

        var duration = Convert.ToSingle(sound.Stream.GetLength());
        DestroyDelay(sound, duration);
    }

    public void Play(AudioStream stream, SoundSettings settings = null)
    {
        if (stream == null) return;

        var sound = new AudioStreamPlayer();
        sound.SetParent(Scene.Root);
        sound.Stream = stream;
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
    public float MaxDistance { get; set; } = 16f;
    public float UnitSize { get; set; } = 10f;

    public void ModifySound(AudioStreamPlayer3D sound)
    {
        sound.Bus = Bus.ToString();
        sound.GlobalPosition = Position;
        sound.PitchScale = RandomPitch;
        sound.VolumeDb = Volume;
        sound.MaxDistance = MaxDistance;
        sound.UnitSize = UnitSize;
    }
}

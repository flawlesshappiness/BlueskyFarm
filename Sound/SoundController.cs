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

    public int GetBusIndex(SoundBus bus)
    {
        return AudioServer.GetBusIndex(bus.ToString());
    }

    public int GetBusEffectIndex<T>(SoundBus bus) where T : AudioEffect
    {
        var idx = GetBusIndex(bus);
        var count = AudioServer.GetBusEffectCount(idx);
        for (int i = 0; i < count; i++)
        {
            var effect = AudioServer.GetBusEffect(idx, i) as T;
            if (effect != null) return i;
        }

        return -1;
    }

    public T GetBusEffect<T>(SoundBus bus) where T : AudioEffect
    {
        var idx = GetBusIndex(bus);
        var count = AudioServer.GetBusEffectCount(idx);
        for (int i = 0; i < count; i++)
        {
            var effect = AudioServer.GetBusEffect(idx, i) as T;
            if (effect != null) return effect;
        }

        return null;
    }

    public void SetBusEffectEnabled<T>(SoundBus bus, bool enabled) where T : AudioEffect
    {
        var idx_effect = GetBusEffectIndex<T>(bus);
        if (idx_effect >= 0)
        {
            var idx_bus = GetBusIndex(bus);
            AudioServer.SetBusEffectEnabled(idx_bus, idx_effect, enabled);
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

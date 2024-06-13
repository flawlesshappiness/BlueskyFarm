using Godot;
using System.Collections;
using System.Linq;

public partial class SoundController : ResourceController<SoundCollection, SoundInfo>
{
    public override string Directory => "Sound";
    public static SoundController Instance => Singleton.Get<SoundController>();

    public SoundInfo GetInfo(SoundName name) => Collection.Resources.FirstOrDefault(x => x.SoundName == name);

    public void Play(SoundName name, PlaySoundSettings settings = null)
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

public class PlaySoundSettings
{
    public float PitchMin { get; set; } = 1;
    public float PitchMax { get; set; } = 1;

    public void ModifySound(AudioStreamPlayer sound)
    {
        var rng = new RandomNumberGenerator();
        sound.PitchScale = rng.RandfRange(PitchMin, PitchMax);
    }
}

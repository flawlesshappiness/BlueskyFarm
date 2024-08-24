using Godot;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class AmbienceController : ResourceController<AmbienceGroupCollection, AmbienceGroupInfo>
{
    public static AmbienceController Instance => Singleton.Get<AmbienceController>();
    public override string Directory => "Ambience";

    private List<Coroutine> _coroutines = new();

    public void StartAmbience(string group_name)
    {
        StopAmbience();

        var infos = Collection.Resources.Where(x => x.Name == group_name);
        foreach (var info in infos)
        {
            var cr = Coroutine.Start(CrAmbience(info));
            _coroutines.Add(cr);
        }
    }

    public void StopAmbience()
    {
        foreach (var cr in _coroutines)
        {
            Coroutine.Stop(cr);
        }

        _coroutines.Clear();
    }

    private IEnumerator CrAmbience(AmbienceGroupInfo info)
    {
        var rng = new RandomNumberGenerator();
        var mul_debug = 1f;
        yield return new WaitForSeconds(rng.RandfRange(15, 30) * mul_debug);

        AudioStream previous = null;
        while (true)
        {
            var sound = info.Sounds
                .Where(x => x != previous || info.Sounds.Count == 1)
                .ToList()
                .Random();
            var x = rng.RandfRange(-1, 1);
            var z = rng.RandfRange(-1, 1);
            var distance = rng.RandfRange(info.DistanceMin, info.DistanceMax);
            var offset = new Vector3(x, 0, z).Normalized() * distance;
            var position = FirstPersonController.Instance.GlobalPosition + offset;
            SoundController.Instance.Play(sound, new SoundSettings3D
            {
                Bus = SoundBus.SFX,
                MaxDistance = distance * 2,
                Position = position,
                Volume = rng.RandfRange(info.VolumeMin, info.VolumeMax),
                PitchMin = info.PitchMin,
                PitchMax = info.PitchMax
            });

            var delay = rng.RandfRange(15, 30) * mul_debug;
            yield return new WaitForSeconds(sound.GetLength() + delay);

            previous = sound;
        }
    }
}

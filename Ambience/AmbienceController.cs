using Godot;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class AmbienceController : ResourceController<AmbienceGroupCollection, AmbienceGroupInfo>
{
    public static AmbienceController Instance => Singleton.Get<AmbienceController>();
    public override string Directory => "Ambience";

    private List<Coroutine> _coroutines = new();

    public void Start(string group_name)
    {
        Stop();

        var infos = Collection.Resources.Where(x => x.Name == group_name);
        foreach (var info in infos)
        {
            var cr = Coroutine.Start(CrAmbience(info));
            _coroutines.Add(cr);
        }
    }

    public void Stop()
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

        SoundInfo previous = null;
        while (true)
        {
            var allow_duplicate = info.Infos.Count == 1;
            var sound = info.Infos
                .Where(x => allow_duplicate || x != previous)
                .ToList()
                .Random();
            var x = rng.RandfRange(-1, 1);
            var z = rng.RandfRange(-1, 1);
            var distance = rng.RandfRange(12, 20);
            var offset = new Vector3(x, 0, z).Normalized() * distance;
            var position = Player.Instance.GlobalPosition + offset;
            var asp = SoundController.Instance.Play(sound, position);

            var delay = rng.RandfRange(15, 30) * mul_debug;
            var length = asp.Stream.GetLength();
            yield return new WaitForSeconds(length + delay);
            previous = sound;
        }
    }
}

using Godot;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class AmbienceController : ResourceController<AmbienceCollection, AmbienceInfo>
{
    public static AmbienceController Instance => Singleton.Get<AmbienceController>();
    public override string Directory => "Ambience";
    public string CurrentArea { get; private set; }
    public AmbienceInfo CurrentInfo { get; private set; }

    private AudioStreamPlayer _current_background_asp;
    private List<Coroutine> _cr_noises = new();

    private const float FADE_TIME = 1f;

    protected override void Initialize()
    {
        base.Initialize();
        BasementController.Instance.OnBasementExited += BasementExited;
        BasementController.Instance.OnRoomEntered += RoomEntered;
    }

    private void BasementExited()
    {
        StopAmbientNoise();
    }

    private void RoomEntered(BasementRoomElement element)
    {
        var area = element.Info.AmbienceArea;
        if (CurrentArea == area) return;

        CurrentInfo = GetInfo(area);
        if (CurrentInfo == null) return;

        CurrentArea = area;
        StartBackgroundAmbience();
        StartAmbientNoise();
        FadeEffects();
    }

    private AmbienceInfo GetInfo(string area)
    {
        return Collection.Resources.FirstOrDefault(x => x.Area == area);
    }

    public void StartAmbienceImmediate(string area)
    {
        if (_current_background_asp != null)
        {
            _current_background_asp.QueueFree();
        }

        CurrentInfo = GetInfo(area);
        if (CurrentInfo == null) return;

        CurrentArea = area;
        _current_background_asp = SoundController.Instance.Play(CurrentInfo.BackgroundSound);
        StartAmbientNoise();
        SetEffectsImmediate();
    }

    // Background ambience
    public void StartBackgroundAmbience()
    {
        if (_current_background_asp != null)
        {
            FadeOutThenDestroy(_current_background_asp);
        }

        if (CurrentInfo == null)
        {
            _current_background_asp = null;
        }
        else
        {
            _current_background_asp = SoundController.Instance.Play(CurrentInfo.BackgroundSound, new SoundOverride
            {
                Volume = -80f
            });

            _current_background_asp.FadeIn(FADE_TIME, CurrentInfo.BackgroundSound.Volume);
        }
    }

    private void FadeOutThenDestroy(AudioStreamPlayer asp)
    {
        Coroutine.Start(Cr, asp);
        IEnumerator Cr()
        {
            yield return asp.FadeOut(FADE_TIME);
            asp.QueueFree();
        }
    }

    // Noises
    public void StartAmbientNoise()
    {
        StopAmbientNoise();
        if (CurrentInfo == null) return;

        foreach (var noise in CurrentInfo.Noises)
        {
            var cr = Coroutine.Start(Cr(noise));
            _cr_noises.Add(cr);
        }

        IEnumerator Cr(SoundInfo noise)
        {
            var rng = new RandomNumberGenerator();
            var mul_debug = 1f;
            yield return new WaitForSeconds(rng.RandfRange(15, 30) * mul_debug);

            while (true)
            {
                var x = rng.RandfRange(-1, 1);
                var z = rng.RandfRange(-1, 1);
                var distance = rng.RandfRange(12, 20);
                var offset = new Vector3(x, 0, z).Normalized() * distance;
                var position = Player.Instance.GlobalPosition + offset;
                var asp = SoundController.Instance.Play(noise, position);
                var delay = rng.RandfRange(15, 30) * mul_debug;
                var length = asp.Stream.GetLength();
                yield return new WaitForSeconds(length + delay);
            }
        }
    }

    public void StopAmbientNoise()
    {
        foreach (var cr in _cr_noises)
        {
            Coroutine.Stop(cr);
        }

        _cr_noises.Clear();
    }

    // Effects
    public void SetEffectsImmediate()
    {
        SetReverb(CurrentInfo.ReverbAmount);
    }

    public void FadeEffects()
    {
        FadeReverb();
    }

    public void SetReverb(float value)
    {
        var bus = AudioBus.Get(SoundBus.SFX.ToString());
        bus.SetEffectEnabled<AudioEffectReverb>(true);
        var reverb = bus.GetEffect<AudioEffectReverb>();
        reverb.Wet = value;
    }

    public void FadeReverb()
    {
        Coroutine.Start(Cr, nameof(FadeReverb));
        IEnumerator Cr()
        {
            var bus = AudioBus.Get(SoundBus.SFX.ToString());
            bus.SetEffectEnabled<AudioEffectReverb>(true);
            var reverb = bus.GetEffect<AudioEffectReverb>();
            var start = reverb.Wet;
            var end = CurrentInfo.ReverbAmount;

            yield return LerpEnumerator.Lerp01(2f, f =>
            {
                var v = Mathf.Lerp(start, end, f);
                reverb.Wet = v;
            });
        }
    }
}

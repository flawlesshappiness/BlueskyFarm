using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class AmbienceController : ResourceController<AmbienceCollection, AmbienceInfo>
{
    public static AmbienceController Instance => Singleton.Get<AmbienceController>();
    public override string Directory => "Ambience";
    public string CurrentArea { get; private set; }
    public AmbienceInfo CurrentInfo { get; private set; }

    private float _time_bgm_area_enter;
    private AudioStreamPlayer _current_background_asp;
    private List<Coroutine> _cr_noises = new();

    private const float FADE_TIME = 1f;

    public void StartAmbience(AreaNameType type) => StartAmbience(type.ToString());
    public void StartAmbience(string area)
    {
        if (CurrentArea == area) return;

        CurrentInfo = GetInfo(area);
        if (CurrentInfo == null) return;

        CurrentArea = area;
        StartBackgroundAmbience();
        StartAmbientNoise();
        PlayAreaEnterMusic();
        FadeEffects();
    }

    public void StopAmbience()
    {
        StopBackgroundAmbience();
        StopAmbientNoise();
    }

    private AmbienceInfo GetInfo(string area)
    {
        return Collection.Resources.FirstOrDefault(x => x.Area.ToString() == area);
    }

    private void PlayAreaEnterMusic()
    {
        if (GameTime.Time < _time_bgm_area_enter) return;

        var sfx = $"sfx_enter_{CurrentArea}".ToLower();
        var info = SoundController.Instance.Collection.GetResource(sfx);
        if (info == null) return;

        this.StartCoroutine(Cr, "area_enter");
        IEnumerator Cr()
        {
            yield return new WaitForSeconds(5f);

            var asp = SoundController.Instance.Play(info);
            var stream = asp.Stream;

            _time_bgm_area_enter = GameTime.Time + Convert.ToSingle(stream.GetLength());
        }
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
        PlayAreaEnterMusic();
    }

    // Background ambience
    public void StartBackgroundAmbience()
    {
        StopBackgroundAmbience();

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

    public void StopBackgroundAmbience()
    {
        if (_current_background_asp != null)
        {
            FadeOutThenDestroy(_current_background_asp);
            _current_background_asp = null;
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
            var mul_debug = 1.0f;
            yield return new WaitForSeconds(rng.RandfRange(15, 30) * mul_debug);

            while (true)
            {
                var position = GetAmbientSoundPosition();
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

    public Vector3 GetAmbientSoundPosition()
    {
        var rng = new RandomNumberGenerator();
        var x = rng.RandfRange(-1, 1);
        var z = rng.RandfRange(-1, 1);
        var distance = rng.RandfRange(12, 20);
        var offset = new Vector3(x, 0, z).Normalized() * distance;
        var position = Player.Instance.GlobalPosition + offset;
        return position;
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

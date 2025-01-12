using Godot;
using System.Collections;
using System.Linq;

public partial class AmbienceBackgroundController : ResourceController<AmbienceBackgroundCollection, AmbienceBackgroundInfo>
{
    public override string Directory => "AmbienceBackground";

    private string _current_area;
    private AudioStreamPlayer _current_asp;

    private const float FADE_TIME = 1f;

    protected override void Initialize()
    {
        base.Initialize();
        BasementController.Instance.OnBasementEntered += BasementEntered;
        BasementController.Instance.OnRoomEntered += RoomEntered;
    }

    private void BasementEntered()
    {
        if (_current_asp != null)
        {
            _current_asp.QueueFree();
        }

        _current_area = AreaNames.Basement;
        var info = GetInfo(_current_area);
        _current_asp = SoundController.Instance.Play(info.Sound, new SoundOverride
        {

        });
    }

    public void RoomEntered(BasementRoomElement element)
    {
        var area = element.Info.AmbienceArea;
        if (_current_area == area) return;

        _current_area = area;
        StartBackgroundAmbience(_current_area);
    }

    private AmbienceBackgroundInfo GetInfo(string area)
    {
        return Collection.Resources.FirstOrDefault(x => x.Area == area);
    }

    public void StartBackgroundAmbience(string area)
    {
        if (_current_asp != null)
        {
            FadeOutThenDestroy(_current_asp);
        }

        var info = GetInfo(_current_area);
        if (info == null)
        {
            _current_asp = null;
        }
        else
        {
            _current_asp = SoundController.Instance.Play(info.Sound, new SoundOverride
            {
                Volume = -80f
            });

            _current_asp.FadeIn(FADE_TIME, info.Sound.Volume);
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
}

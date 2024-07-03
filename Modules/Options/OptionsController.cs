using Godot;
using System;

public partial class OptionsController : SingletonController
{
    public override string Directory => $"{Paths.Modules}/Options";
    public static OptionsController Instance => Singleton.Get<OptionsController>();

    protected override void Initialize()
    {
        base.Initialize();

        LoadData();
    }

    private void LoadData()
    {
        UpdateVolume("Master", Data.Game.VolumeMaster);
        UpdateVolume("SFX", Data.Game.VolumeSFX);
        UpdateVolume("BGM", Data.Game.VolumeBGM);
    }

    public void UpdateVolume(string name, float value)
    {
        var bus = AudioBus.Get(name);
        bus.SetVolume(PercentageToDecibel(value));

        SoundController.Instance.Play(SoundName.Step_Default, new SoundSettings
        {
            Bus = SoundBus.SFX,
        });
    }

    public float PercentageToDecibel(float value)
    {
        var t = Mathf.Clamp(value, 0.0001f, 1.0f);
        var dcb = Math.Log10(t) * 20;
        return Convert.ToSingle(dcb);
    }
}

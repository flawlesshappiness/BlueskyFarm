using Godot;
using System;

public partial class OptionsView : View
{
    public override string Directory => $"{Paths.Modules}/Options/View";

    [NodeName(nameof(BackButton))]
    public Button BackButton;

    [NodeName(nameof(MasterSlider))]
    public Slider MasterSlider;

    [NodeName(nameof(SFXSlider))]
    public Slider SFXSlider;

    [NodeName(nameof(BGMSlider))]
    public Slider BGMSlider;

    public Action OnBack;

    public override void _Ready()
    {
        base._Ready();

        MasterSlider.Value = Data.Game.VolumeMaster;
        SFXSlider.Value = Data.Game.VolumeSFX;
        BGMSlider.Value = Data.Game.VolumeBGM;

        BackButton.Pressed += BackPressed;
        MasterSlider.ValueChanged += MasterSlider_ValueChanged;
        SFXSlider.ValueChanged += SFXSlider_ValueChanged;
        BGMSlider.ValueChanged += BGMSlider_ValueChanged;
    }

    protected override void OnShow()
    {
        base.OnShow();
        Scene.PauseLock.AddLock(nameof(OptionsView));
        MouseVisibility.Instance.Lock.AddLock(nameof(OptionsView));
    }

    protected override void OnHide()
    {
        base.OnHide();
        Scene.PauseLock.RemoveLock(nameof(OptionsView));
        MouseVisibility.Instance.Lock.RemoveLock(nameof(OptionsView));
    }

    private void BackPressed()
    {
        Hide();
        OnBack?.Invoke();
    }

    private void MasterSlider_ValueChanged(double v)
    {
        var f = Convert.ToSingle(v);
        OptionsController.Instance.UpdateVolume("Master", f);
        Data.Game.VolumeMaster = f;
    }

    private void SFXSlider_ValueChanged(double v)
    {
        var f = Convert.ToSingle(v);
        OptionsController.Instance.UpdateVolume("SFX", f);
        Data.Game.VolumeSFX = f;
    }

    private void BGMSlider_ValueChanged(double v)
    {
        var f = Convert.ToSingle(v);
        OptionsController.Instance.UpdateVolume("BGM", f);
        Data.Game.VolumeBGM = f;
    }
}

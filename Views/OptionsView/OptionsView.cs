using Godot;
using System;

public partial class OptionsView : View
{
    public override string Directory => $"{Paths.ViewDirectory}/OptionsView";

    [NodeType]
    public OptionsControl OptionsControl;

    [NodeName]
    public Label VersionLabel;

    [NodeName]
    public Slider BrightnessSlider;

    public override void _Ready()
    {
        base._Ready();

        BrightnessSlider.Value = Data.Options.Brightness;

        BrightnessSlider.ValueChanged += BrightnessSlider_ValueChanged;
        OptionsControl.OnBack += BackPressed;
    }

    private void BackPressed()
    {
        Hide();
    }

    protected override void OnShow()
    {
        base.OnShow();

        MouseVisibility.Instance.Lock.AddLock(nameof(OptionsView));

        UpdateVersionLabel();
    }

    protected override void OnHide()
    {
        base.OnHide();

        MouseVisibility.Instance.Lock.RemoveLock(nameof(OptionsView));
    }

    private void UpdateVersionLabel()
    {
        var released = ApplicationInfo.Instance.Release ? "RELEASE" : "BETA";
        var version = ApplicationInfo.Instance.Version;
        VersionLabel.Text = $"{released} {version}";
    }

    private void BrightnessSlider_ValueChanged(double v)
    {
        var f = Convert.ToSingle(v);
        OptionsController.Instance.UpdateBrightness(f);
        Data.Options.Brightness = f;
    }
}

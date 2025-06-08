using Godot;
using System;

public partial class OptionsView : View
{
    public override string Directory => $"{Paths.ViewDirectory}/OptionsView";

    [Export]
    public SoundInfo HoverSound;

    [Export]
    public SoundInfo PressedSound;

    [NodeType]
    public OptionsControl OptionsControl;

    [NodeName]
    public Label VersionLabel;

    [NodeName]
    public Slider BrightnessSlider;

    [NodeName]
    public Label MouseSensitivityLabel;

    [NodeName]
    public Slider MouseSensitivitySlider;

    [NodeName]
    public Label HeadbobAmountLabel;

    [NodeName]
    public Slider HeadbobAmountSlider;

    [Export]
    public Button StuckFixButton;

    public override void _Ready()
    {
        base._Ready();

        BrightnessSlider.ValueChanged += BrightnessSlider_ValueChanged;
        BrightnessSlider.Value = Data.Options.Brightness;

        MouseSensitivitySlider.ValueChanged += MouseSensitivitySlider_ValueChanged;
        MouseSensitivitySlider.Value = Data.Options.MouseSensitivity;

        HeadbobAmountSlider.ValueChanged += HeadbobAmountSlider_ValueChanged;
        HeadbobAmountSlider.Value = Data.Options.HeadbobAmount;

        StuckFixButton.Pressed += StuckFixButton_Pressed;

        OptionsControl.OnBack += BackPressed;

        InitializeSounds();
    }

    private void InitializeSounds()
    {
        Rec(OptionsControl);

        void Rec(Node node)
        {
            foreach (var child in node.GetChildren())
            {
                if (child is Slider slider && slider != null)
                {
                    slider.MouseEntered += PlayHoverSfx;
                }
                else if (child is Button button && button != null)
                {
                    button.MouseEntered += PlayHoverSfx;
                    button.Pressed += PlayPressedSfx;
                }
                else if (child is OptionButton dropdown && dropdown != null)
                {
                    dropdown.MouseEntered += PlayHoverSfx;
                    dropdown.Pressed += PlayPressedSfx;
                    dropdown.ItemSelected += _ => PlayPressedSfx();
                }
                else if (child is TabContainer tabs && tabs != null)
                {
                    tabs.TabHovered += _ => PlayHoverSfx();
                    tabs.TabClicked += _ => PlayPressedSfx();
                }

                Rec(child);
            }
        }

        void PlayHoverSfx()
        {
            SoundController.Instance.Play(HoverSound);
        }

        void PlayPressedSfx()
        {
            SoundController.Instance.Play(PressedSound);
        }
    }

    private void BackPressed()
    {
        Hide();
    }

    protected override void OnShow()
    {
        base.OnShow();

        MouseVisibility.Instance.Lock.AddLock(nameof(OptionsView));

        StuckFixButton.Disabled = Player.Instance == null;

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

    private void MouseSensitivitySlider_ValueChanged(double v)
    {
        var f = Convert.ToSingle(v);
        MouseSensitivityLabel.Text = $"{f.ToString("0.00")}";
        Data.Options.MouseSensitivity = f;
    }

    private void HeadbobAmountSlider_ValueChanged(double v)
    {
        var f = Convert.ToSingle(v);
        HeadbobAmountLabel.Text = $"{f.ToString("0.00")}";
        Data.Options.HeadbobAmount = f;
    }

    private void StuckFixButton_Pressed()
    {
        Player.Instance.Unstuck();
    }
}

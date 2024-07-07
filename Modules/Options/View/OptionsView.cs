using Godot;
using System;

public partial class OptionsView : View
{
    public override string Directory => $"{Paths.Modules}/Options/View";

    [NodeName]
    public Button BackButton;

    [NodeType]
    public TabContainer Tabs;

    [NodeName]
    public Slider MasterSlider;

    [NodeName]
    public Slider SFXSlider;

    [NodeName]
    public Slider BGMSlider;

    [NodeName]
    public OptionButton WindowModeDropdown;

    [NodeName]
    public Control Resolution;

    [NodeName]
    public OptionButton ResolutionDropdown;

    [NodeName]
    public OptionButton VSyncDropdown;

    [NodeName]
    public OptionButton FPSLimitDropdown;

    [NodeType]
    public OptionsKeys Keys;

    public Action OnBack;

    public override void _Ready()
    {
        base._Ready();

        WindowMode_AddItems();
        Resolution_AddItems();
        Resolution_UpdateVisible();
        VSync_AddItems();
        FPSLimit_AddItems();

        MasterSlider.Value = Data.Game.VolumeMaster;
        SFXSlider.Value = Data.Game.VolumeSFX;
        BGMSlider.Value = Data.Game.VolumeBGM;
        WindowModeDropdown.Selected = Data.Game.WindowMode;
        ResolutionDropdown.Selected = Data.Game.Resolution;
        VSyncDropdown.Selected = Data.Game.VSync;
        FPSLimitDropdown.Selected = Data.Game.FPSLimit;

        BackButton.Pressed += BackPressed;
        MasterSlider.ValueChanged += MasterSlider_ValueChanged;
        SFXSlider.ValueChanged += SFXSlider_ValueChanged;
        BGMSlider.ValueChanged += BGMSlider_ValueChanged;
        WindowModeDropdown.ItemSelected += WindowMode_SelectionChanged;
        ResolutionDropdown.ItemSelected += Resolution_SelectionChanged;
        VSyncDropdown.ItemSelected += VSync_SelectionChanged;
        FPSLimitDropdown.ItemSelected += FPSLimit_SelectionChanged;

        Keys.OnRebindStart += RebindStart;
        Keys.OnRebindEnd += RebindEnd;
    }

    protected override void OnShow()
    {
        base.OnShow();
        Scene.PauseLock.AddLock(nameof(OptionsView));
        MouseVisibility.Instance.Lock.AddLock(nameof(OptionsView));

        Tabs.CurrentTab = 0;
        Keys.UpdateAllKeyStrings();
        Keys.UpdateDuplicateWarnings();
    }

    protected override void OnHide()
    {
        base.OnHide();
        Scene.PauseLock.RemoveLock(nameof(OptionsView));
        MouseVisibility.Instance.Lock.RemoveLock(nameof(OptionsView));
    }

    private void BackPressed()
    {
        OnBack?.Invoke();
        Hide();
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

    private void WindowMode_AddItems()
    {
        foreach (var mode in OptionsController.WindowModes)
        {
            var item = mode switch
            {
                Window.ModeEnum.Windowed => "Windowed",
                Window.ModeEnum.ExclusiveFullscreen => "Fullscreen",
                _ => ""
            };

            WindowModeDropdown.AddItem(item);
        }
    }

    private void WindowMode_SelectionChanged(long index)
    {
        var i = (int)index;
        if (OptionsController.WindowModes.GetClamped(i) == Window.ModeEnum.Windowed)
        {
            OptionsController.Instance.UpdateResolution(i);
        }

        OptionsController.Instance.UpdateWindowMode(i);
        Data.Game.WindowMode = i;
        Resolution_UpdateVisible();
    }

    private void Resolution_UpdateVisible()
    {
        var mode = OptionsController.WindowModes.GetClamped(Data.Game.WindowMode);
        Resolution.Visible = mode == Window.ModeEnum.Windowed;
    }

    private void Resolution_AddItems()
    {
        foreach (var res in OptionsController.Resolutions)
        {
            var item = $"{res.X}x{res.Y}";
            ResolutionDropdown.AddItem(item);
        }
    }

    private void Resolution_SelectionChanged(long index)
    {
        var i = (int)index;
        OptionsController.Instance.UpdateResolution(i);
        Data.Game.Resolution = i;
    }

    private void VSync_AddItems()
    {
        foreach (var mode in OptionsController.VSyncModes)
        {
            var item = mode switch
            {
                DisplayServer.VSyncMode.Mailbox => "Fast",
                _ => mode.ToString()
            };

            VSyncDropdown.AddItem(item);
        }
    }

    private void VSync_SelectionChanged(long index)
    {
        var i = (int)index;
        OptionsController.Instance.UpdateVsync(i);
        Data.Game.VSync = i;
    }

    private void FPSLimit_AddItems()
    {
        foreach (var mode in OptionsController.FPSLimits)
        {
            var item = mode switch
            {
                0 => "Unlimited",
                _ => mode.ToString()
            };

            FPSLimitDropdown.AddItem(item);
        }
    }

    private void FPSLimit_SelectionChanged(long index)
    {
        var i = (int)index;
        OptionsController.Instance.UpdateFPSLimit(i);
        Data.Game.FPSLimit = i;
    }

    private void RebindStart()
    {
        BackButton.Disabled = true;
        SetTabsEnabled(false);
    }

    private void RebindEnd()
    {
        BackButton.Disabled = false;
        SetTabsEnabled(true);
    }

    private void SetTabsEnabled(bool enabled)
    {
        for (int i = 0; i < Tabs.GetTabCount(); i++)
        {
            Tabs.SetTabDisabled(i, !enabled);
        }
    }
}

using Godot;
using System;
using System.Collections.Generic;

public partial class OptionsController : SingletonController
{
    public override string Directory => $"{Paths.Modules}/Options";
    public static OptionsController Instance => Singleton.Get<OptionsController>();
    public static Window.ModeEnum CurrentWindowMode => WindowModes.GetClamped(Data.Game.WindowMode);
    public static DisplayServer.VSyncMode CurrentVSyncMode => VSyncModes.GetClamped(Data.Game.VSync);

    public static readonly List<Window.ModeEnum> WindowModes = new()
    {
        Window.ModeEnum.Windowed,
        Window.ModeEnum.ExclusiveFullscreen
    };

    public static readonly List<Vector2I> Resolutions = new()
    {
        new Vector2I(640, 480),
        new Vector2I(800, 600),
        new Vector2I(1366, 768),
        new Vector2I(1600, 900),
        new Vector2I(1920, 1080),
        new Vector2I(1920, 1200),
        new Vector2I(2560, 1440),
        new Vector2I(2560, 1600),
        new Vector2I(3840, 2160),
    };

    public static readonly List<DisplayServer.VSyncMode> VSyncModes = new()
    {
        DisplayServer.VSyncMode.Disabled,
        DisplayServer.VSyncMode.Adaptive,
        DisplayServer.VSyncMode.Enabled,
        DisplayServer.VSyncMode.Mailbox
    };

    public static readonly List<int> FPSLimits = new()
    {
        30, 60, 120, 144, 0
    };

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
        UpdateVsync(Data.Game.VSync);
        UpdateFPSLimit(Data.Game.FPSLimit);

        if (CurrentWindowMode == Window.ModeEnum.Windowed)
        {
            UpdateResolution(Data.Game.Resolution);
        }

        UpdateWindowMode(Data.Game.WindowMode);
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

    public void UpdateWindowMode(int i)
    {
        var mode = WindowModes.GetClamped(i);
        Scene.Root.Mode = mode;
    }

    public void UpdateResolution(int i)
    {
        var res = Resolutions.GetClamped(i);
        Scene.Root.Size = res;
    }

    public void UpdateVsync(int i)
    {
        var mode = VSyncModes.GetClamped(i);
        DisplayServer.WindowSetVsyncMode(mode);
    }

    public void UpdateFPSLimit(int i)
    {
        var mode = FPSLimits.GetClamped(i);
        Engine.MaxFps = mode;
    }
}

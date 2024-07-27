using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class OptionsController : SingletonController
{
    public override string Directory => $"{Paths.Modules}/Options";
    public static OptionsController Instance => Singleton.Get<OptionsController>();
    public static Window.ModeEnum CurrentWindowMode => WindowModes.GetClamped(Data.Game.WindowMode);
    public static DisplayServer.VSyncMode CurrentVSyncMode => VSyncModes.GetClamped(Data.Game.VSync);

    public event Action OnBrightnessChanged;

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

    public override void _Ready()
    {
        base._Ready();
        Data.Game.OnBeforeSave += BeforeSave;
    }

    protected override void Initialize()
    {
        base.Initialize();

        LoadData();
    }

    private void BeforeSave()
    {
        var view = View.Get<OptionsView>();
        var keys = view.Keys;
        var key_overrides = keys.Rebinds
            .Select(x => x?.Data as InputEventKeyData)
            .Where(x => x != null)
            .ToList();
        var mouse_button_overrides = keys.Rebinds
            .Select(x => x?.Data as InputEventMouseButtonData)
            .Where(x => x != null)
            .ToList();

        Data.Game.KeyOverrides = key_overrides;
        Data.Game.MouseButtonOverrides = mouse_button_overrides;
    }

    private void LoadData()
    {
        LoadActionOverrides();
        UpdateVolume("Master", Data.Game.VolumeMaster);
        UpdateVolume("SFX", Data.Game.VolumeSFX);
        UpdateVolume("BGM", Data.Game.VolumeBGM);
        UpdateVsync(Data.Game.VSync);
        UpdateFPSLimit(Data.Game.FPSLimit);
        UpdateBrightness(Data.Game.Brightness);

        if (CurrentWindowMode == Window.ModeEnum.Windowed)
        {
            UpdateResolution(Data.Game.Resolution);
        }

        UpdateWindowMode(Data.Game.WindowMode);
    }

    private void LoadActionOverrides()
    {
        var view = View.Get<OptionsView>();
        view.Keys.PersistDefaultBindings();

        foreach (var action_override in Data.Game.KeyOverrides)
        {
            UpdateKeyOverride(action_override);
        }

        foreach (var action_override in Data.Game.MouseButtonOverrides)
        {
            UpdateMouseButtonOverride(action_override);
        }
    }

    public void UpdateVolume(string name, float value)
    {
        var bus = AudioBus.Get(name);
        bus.SetVolume(PercentageToDecibel(value));
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

    public void UpdateActionOverride(string action, InputEvent e)
    {
        InputMap.ActionEraseEvents(action);
        InputMap.ActionAddEvent(action, e);
    }

    public void UpdateKeyOverride(InputEventKeyData data)
    {
        Debug.LogMethod($"{data.Action}: {data.Key}");
        Debug.Indent++;

        UpdateActionOverride(data.Action, data.ToEvent());

        Debug.Indent--;
    }

    public void UpdateMouseButtonOverride(InputEventMouseButtonData data)
    {
        Debug.LogMethod($"{data.Action}: {data.Button}");
        Debug.Indent++;

        UpdateActionOverride(data.Action, data.ToEvent());

        Debug.Indent--;
    }

    public void UpdateBrightness(float value)
    {
        OnBrightnessChanged?.Invoke();
    }
}

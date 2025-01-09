using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class OptionsController : SingletonController
{
    public override string Directory => $"{Paths.Modules}/Options";
    public static OptionsController Instance => Singleton.Get<OptionsController>();
    public static Window.ModeEnum CurrentWindowMode => WindowModes.GetClamped(Data.Options.WindowMode);
    public static DisplayServer.VSyncMode CurrentVSyncMode => VSyncModes.GetClamped(Data.Options.VSync);
    public static Dictionary<string, InputEvent> DefaultBindings { get; set; } = new();
    public static List<OptionsKeyRebind> Rebinds { get; set; } = new();

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
        Data.Options.OnBeforeSave += BeforeSave;
    }

    protected override void Initialize()
    {
        base.Initialize();

        LoadData();
    }

    private void BeforeSave()
    {
        var view = View.Get<OptionsView>();
        var key_overrides = Rebinds
            .Select(x => x?.Data as InputEventKeyData)
            .Where(x => x != null)
            .ToList();
        var mouse_button_overrides = Rebinds
            .Select(x => x?.Data as InputEventMouseButtonData)
            .Where(x => x != null)
            .ToList();

        Data.Options.KeyOverrides = key_overrides;
        Data.Options.MouseButtonOverrides = mouse_button_overrides;
    }

    private void LoadData()
    {
        LoadActionOverrides();
        UpdateVolume("Master", Data.Options.VolumeMaster);
        UpdateVolume("SFX", Data.Options.VolumeSFX);
        UpdateVolume("BGM", Data.Options.VolumeBGM);
        UpdateVsync(Data.Options.VSync);
        UpdateFPSLimit(Data.Options.FPSLimit);
        UpdateBrightness(Data.Options.Brightness);

        if (CurrentWindowMode == Window.ModeEnum.Windowed)
        {
            UpdateResolution(Data.Options.Resolution);
        }

        UpdateWindowMode(Data.Options.WindowMode);
    }

    private void LoadActionOverrides()
    {
        PersistDefaultBindings();

        foreach (var action_override in Data.Options.KeyOverrides)
        {
            UpdateKeyOverride(action_override);
        }

        foreach (var action_override in Data.Options.MouseButtonOverrides)
        {
            UpdateMouseButtonOverride(action_override);
        }
    }

    public void PersistDefaultBindings()
    {
        DefaultBindings.Clear();
        var actions = InputMap.GetActions();
        foreach (var action in actions)
        {
            var e = InputMap.ActionGetEvents(action).FirstOrDefault();
            if (e == null) continue;
            DefaultBindings.Add(action, e);
        }
    }

    public void UpdateVolume(string name, float value)
    {
        var bus = AudioBus.Get(name);
        bus.SetVolume(AudioMath.PercentageToDecibel(value));
    }

    public void UpdateWindowMode(int i)
    {
        var mode = WindowModes.GetClamped(i);
        Scene.Root.Mode = mode;

        if (mode == Window.ModeEnum.Windowed)
        {
            UpdateResolution(Data.Options.Resolution);
        }
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

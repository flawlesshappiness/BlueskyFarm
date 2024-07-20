using Godot;
using System.Linq;

public partial class BasementScene : GameScene
{
    private float time_scene_started;

    protected override void Initialize()
    {
        base.Initialize();
        time_scene_started = GameTime.Time;

        // Basement
        BasementController.Instance.DebugGenerateBasement();

        // Data
        Data.Game.OnBeforeSave += OnBeforeSave;

        // Audio
        SetAudioEffectEnabled(true);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        // Data
        Data.Game.OnBeforeSave -= OnBeforeSave;

        // Audio
        SetAudioEffectEnabled(false);
    }

    private void OnBeforeSave()
    {
        ApplyTimePassed();
    }

    private void ApplyTimePassed()
    {
        var time_passed = GameTime.Time - time_scene_started;
        var plant_areas = Data.Game.Scenes.SelectMany(x => x.PlantAreas);

        foreach (var plant_area in plant_areas)
        {
            plant_area.TimeLeft -= time_passed;
        }
    }

    private void SetAudioEffectEnabled(bool enabled)
    {
        var bus = AudioBus.Get("SFX");
        bus.SetEffectEnabled<AudioEffectReverb>(enabled);
    }
}

using Godot;
using System.Collections;
using System.Linq;

public partial class BasementScene : GameScene
{
    [NodeType]
    public NavigationRegion3D NavRegion;

    [Export]
    public Environment BasementEnvironment;

    [Export]
    public Environment ForestEnvironment;

    private float time_scene_started;
    private static bool _registered_debug_actions;

    protected override void Initialize()
    {
        base.Initialize();
        time_scene_started = GameTime.Time;

        RegisterDebugActions();

        // Basement
        BasementController.Instance.GenerateBasement(new BasementSettings
        {
            RoomParent = NavRegion,
            Areas = new()
            {
                new BasementSettingsArea
                {
                    AreaName = "Basement",
                    MaxRooms = 5,
                    MaxCorridorDepth = 3,
                    PuzzleCount = 3,
                },
                new BasementSettingsArea
                {
                    AreaName = "Forest",
                    ConnectedAreaName = "Basement",
                    MaxRooms = 5,
                    MaxCorridorDepth = 3,
                    PuzzleCount = 0,
                }
            }
        });

        // Audio
        SetAudioEffectEnabled(true);
        AmbienceController.Instance.StartAmbience("Basement");

        // Navigation
        NavRegion.BakeNavigationMesh();

        // Enemies
        EnemyController.Instance.SpawnEnemies();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        // Audio
        SetAudioEffectEnabled(false);
        AmbienceController.Instance.StopAmbience();
    }

    protected override void BeforeSave()
    {
        base.BeforeSave();
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

    private void RegisterDebugActions()
    {
        if (_registered_debug_actions) return;
        _registered_debug_actions = true;

        var category = "BASEMENT";

        Debug.RegisterAction(new DebugAction
        {
            Category = category,
            Text = "Animate Basement environment",
            Action = _ => AnimateToEnvironment(BasementEnvironment, 5f)
        });

        Debug.RegisterAction(new DebugAction
        {
            Category = category,
            Text = "Animate Forest environment",
            Action = _ => AnimateToEnvironment(ForestEnvironment, 5f)
        });

        Debug.RegisterAction(new DebugAction
        {
            Category = category,
            Text = "Set Forest environment",
            Action = _ => WorldEnvironment.Environment = ForestEnvironment
        });

        Debug.RegisterAction(new DebugAction
        {
            Category = category,
            Text = "Set Basement environment",
            Action = _ => WorldEnvironment.Environment = BasementEnvironment
        });
    }

    private void AnimateToEnvironment(Environment env, float duration)
    {
        Coroutine.Start(Cr, "env");
        IEnumerator Cr()
        {
            var cur = WorldEnvironment.Environment.Duplicate() as Environment;
            var lerp = cur.CreateLerp(env);
            WorldEnvironment.Environment = lerp.Result;
            yield return LerpEnumerator.Lerp01(duration, f =>
            {
                lerp.UpdateLerp(f);
            });
        }
    }
}

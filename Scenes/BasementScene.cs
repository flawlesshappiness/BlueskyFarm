using Godot;
using System.Collections;
using System.Linq;

public partial class BasementScene : GameScene
{
    [NodeType]
    public NavigationRegion3D NavRegion;

    [NodeName]
    public Node3D DungeonParent;

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
            RoomParent = DungeonParent,
            Areas = new()
            {
                new BasementSettingsArea
                {
                    AreaName = "Basement",
                    MaxRooms = 5,
                    MaxCorridorDepth = 3,
                    PuzzleCount = 0,
                },
                new BasementSettingsArea
                {
                    AreaName = "Forest",
                    ConnectedAreaName = "Basement",
                    MaxRooms = 10,
                    MaxCorridorDepth = 3,
                    PuzzleCount = 0,
                }
            }
        });

        InitializeContainers();

        // Screen effects
        ScreenEffects.Instance.Clear();
        ScreenEffects.StartHeartbeat();

        // Audio
        SetAudioEffectEnabled(true);
        AmbienceController.Instance.Start(AreaNames.Basement);
        AmbientParticleController.Instance.Start(AreaNames.Basement);

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
        AmbienceController.Instance.Stop();

        // Screen effects
        ScreenEffects.StopHeartbeat();
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

        foreach (var data in plant_areas)
        {
            if (data.TimeLeft > 0)
            {
                data.TimeLeft -= time_passed;
                data.TimeUntilNextWeed -= time_passed;
            }
        }
    }

    private void SetAudioEffectEnabled(bool enabled)
    {
        var bus = AudioBus.Get(SoundBus.SFX.ToString());
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

        Debug.RegisterAction(new DebugAction
        {
            Category = category,
            Text = "Teleport to Forest",
            Action = _ => Player.Instance.GlobalPosition = BasementController.Instance.CurrentBasement.Grid.Elements.FirstOrDefault(x => x.AreaName == AreaNames.Forest).Room.GlobalPosition
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

    private void InitializeContainers()
    {
        var basement_rooms = BasementController.Instance.CurrentBasement.Grid.Elements
            .Where(x => x.AreaName == AreaNames.Basement);

        var containers = basement_rooms
            .SelectMany(x => x.Room.GetNodesInChildren<BasementContainer>())
            .Where(x => x.IsVisibleInTree())
            .ToList();

        if (containers.Count == 0) return;

        var item_count = 5;

        for (var i = 0; i < item_count; i++)
        {
            var item = SeedController.Instance.CreateSeed(ItemType.Vegetable);
            item.SetEnabled(false);
            item.RigidBody.LockPosition_All();

            var container = containers.Random();
            containers.Remove(container);
            container.Item = item;
        }
    }
}

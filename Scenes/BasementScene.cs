using Godot;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class BasementScene : GameScene
{
    [NodeType]
    public NavigationRegion3D NavRegion;

    [NodeName]
    public Node3D DungeonParent;

    private float time_scene_started;
    private static bool _registered_debug_actions;

    protected override void Initialize()
    {
        base.Initialize();
        time_scene_started = GameTime.Time;

        RegisterDebugActions();

        // Ambience
        AmbienceController.Instance.StartAmbienceImmediate(AreaNames.Basement);

        // Environment
        EnvironmentController.Instance.SetEnvironment(AreaNameType.Basement);

        // Basement
        BasementController.Instance.GenerateBasement(new BasementSettings
        {
            RoomParent = DungeonParent,
            Areas = new()
            {
                new BasementSettingsArea
                {
                    AreaName = AreaNames.Basement,
                    MaxRooms = 5,
                    MaxCorridorDepth = 3,
                },
                new BasementSettingsArea
                {
                    AreaName = AreaNames.Forest,
                    ConnectedAreaName = AreaNames.Basement,
                    MaxRooms = 10,
                    MaxCorridorDepth = 3,
                }
            }
        });

        // Containers
        InitializeContainers();

        // Screen effects
        ScreenEffects.Instance.Clear();
        ScreenEffects.StartHeartbeat();

        // Navigation
        NavRegion.NavigationMeshChanged += NavigationMeshChanged;
        NavRegion.BakeNavigationMesh();
    }

    private void NavigationMeshChanged()
    {
        // Enemies
        EnemyController.Instance.SpawnEnemies();

        // Enter
        BasementController.Instance.EnterBasement();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        // Screen effects
        ScreenEffects.StopHeartbeat();

        // Exit
        BasementController.Instance.ExitBasement();
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

    private void RegisterDebugActions()
    {
        if (_registered_debug_actions) return;
        _registered_debug_actions = true;

        var category = "BASEMENT";

        Debug.RegisterAction(new DebugAction
        {
            Category = category,
            Text = "Teleport to area",
            Action = TeleportToSearch
        });

        void TeleportToSearch(DebugView view)
        {
            view.SetContent_Search();

            var types = new List<AreaNameType> { AreaNameType.Basement, AreaNameType.Forest };
            foreach (var type in types)
            {
                view.ContentSearch.AddItem(type.ToString(), () => TeleportTo(view, type));
            }

            view.ContentSearch.UpdateButtons();
        }

        void TeleportTo(DebugView view, AreaNameType area)
        {
            view.SetVisible(false);

            var room = BasementController.Instance.CurrentBasement.Grid.Elements.FirstOrDefault(x => x.AreaName == area.ToString());
            Player.Instance.GlobalPosition = room.Room.GlobalPosition;
        }
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
            item.LockPosition_All();

            var container = containers.Random();
            containers.Remove(container);
            container.Item = item;
        }
    }
}

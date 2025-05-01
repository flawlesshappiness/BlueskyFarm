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
        var settings = new BasementSettings
        {
            RoomParent = DungeonParent,
            Areas = new()
            {
                new BasementSettingsArea
                {
                    AreaName = AreaNames.Basement,
                    MaxRooms = 5,
                    MaxCorridorDepth = 3,
                    SeedType = ItemType.Crop_Vegetable,
                    SeedCount = 5
                },
                new BasementSettingsArea
                {
                    AreaName = AreaNames.Forest,
                    ConnectedAreaName = AreaNames.Basement,
                    MaxRooms = 10,
                    MaxCorridorDepth = 3,
                    SeedType = ItemType.Crop_Bone,
                    SeedCount = 5
                },
                new BasementSettingsArea
                {
                    AreaName = AreaNames.Mine,
                    ConnectedAreaName = AreaNames.Forest,
                    MaxRooms = 10,
                    MaxCorridorDepth = 3,
                    SeedType = ItemType.Crop_Stone,
                    SeedCount = 7
                },
                new BasementSettingsArea
                {
                    AreaName = AreaNames.Cult,
                    ConnectedAreaName = AreaNames.Forest,
                    MaxRooms = 5,
                    MaxCorridorDepth = 3,
                    SeedType = ItemType.Crop_Stone,
                    SeedCount = 7
                }
            }
        };

        BasementController.Instance.GenerateBasement(settings);

        // Items
        InitializeSeeds(settings);
        InitializeRandomBlueprints(settings);

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

        // BGM
        AmbienceController.Instance.StopAreaEnterMusic();

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

        foreach (var data in Data.Game.PlantAreas)
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

            var types = new List<AreaNameType> { AreaNameType.Basement, AreaNameType.Forest, AreaNameType.Mine, AreaNameType.Cult };
            foreach (var type in types)
            {
                view.ContentSearch.AddItem(type.ToString(), () => TeleportTo(view, type));
            }

            view.ContentSearch.UpdateButtons();
        }

        void TeleportTo(DebugView view, AreaNameType area)
        {
            view.Close();

            var room = BasementController.Instance.CurrentBasement.Grid.Elements.FirstOrDefault(x => x.AreaName == area.ToString());
            Player.Instance.GlobalPosition = room.Room.EnemyMarker.GlobalPosition;

            var amb_area = area;
            var env_area = area;
            if (area == AreaNameType.Forest)
            {
                amb_area = AreaNameType.Basement;
                env_area = AreaNameType.Basement;
            }

            EnvironmentController.Instance.SetEnvironment(env_area);
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

    private void InitializeSeeds(BasementSettings settings)
    {
        foreach (var area in settings.Areas)
        {
            var basement_rooms = BasementController.Instance.CurrentBasement.Grid.Elements
                .Where(x => x.AreaName == area.AreaName);

            var containers = basement_rooms
                .SelectMany(x => x.Room.GetNodesInChildren<ItemContainer>())
                .Where(x => x.IsVisibleInTree() && !x.HasItem)
                .TakeRandom(area.SeedCount);

            foreach (var container in containers)
            {
                var item = SeedController.Instance.CreateSeed(area.SeedType);
                container.SetItem(item);
            }
        }
    }

    private void InitializeRandomBlueprints(BasementSettings settings)
    {
        var rng = new RandomNumberGenerator();
        var selected_bps = new List<BlueprintInfo>();

        foreach (var area in settings.Areas)
        {
            if (rng.Randf() > 0.5f) continue;

            var basement_rooms = BasementController.Instance.CurrentBasement.Grid.Elements
                .Where(x => x.AreaName == area.AreaName);

            var container = basement_rooms
                .SelectMany(x => x.Room.GetNodesInChildren<ItemContainer>())
                .Where(x => x.IsVisibleInTree() && !x.HasItem)
                .ToList()
                .Random();

            if (container == null) continue;

            //Debug.Log(area.AreaName);
            var blueprints_not_selected = BlueprintController.Instance.Collection.Resources.Where(x => !selected_bps.Contains(x));
            //Debug.Log($"{nameof(blueprints_not_selected)}: {blueprints_not_selected.Count()}");
            var blueprints_area = blueprints_not_selected.Where(x => x.Areas != null && x.Areas.Any(bp_area => bp_area.ToString() == area.AreaName));
            //Debug.Log($"{nameof(blueprints_area)}: {blueprints_area.Count()}");
            var blueprints_access = blueprints_area.Where(x => !Player.HasAccessToBlueprint(x.Id) && !Player.HasAccessToItem(x.ResultItemInfo));
            //Debug.Log($"{nameof(blueprints_access)}: {blueprints_access.Count()}");
            var blueprints = blueprints_access.ToList();

            RemoveUnlockedBlueprints(blueprints);
            //Debug.Log($"{nameof(blueprints)}: {blueprints.Count()}");

            var blueprint = blueprints
                .Random();

            if (blueprint == null) continue;

            selected_bps.Add(blueprint);

            var item = BlueprintController.Instance.CreateBlueprintRoll(blueprint.Id);
            container.SetItem(item);
        }
    }

    private void RemoveUnlockedBlueprints(List<BlueprintInfo> blueprints)
    {
        foreach (var bp in blueprints.ToList())
        {
            if (HasCraftedBlueprintBefore(bp.Id))
            {
                blueprints.Remove(bp);
            }
        }
    }

    private bool HasCraftedBlueprintBefore(string id)
    {
        return Data.Game.BlueprintCraftedCounts.Any(x => x.Id == id && x.Count > 0);
    }
}

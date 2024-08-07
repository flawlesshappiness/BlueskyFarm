using Godot;
using System.Linq;

public partial class BasementScene : GameScene
{
    [NodeType]
    public NavigationRegion3D NavRegion;

    private float time_scene_started;

    protected override void Initialize()
    {
        base.Initialize();
        time_scene_started = GameTime.Time;

        // Basement
        BasementController.Instance.GenerateBasement(new BasementSettings
        {
            RoomParent = NavRegion,
            MaxRooms = 5,
            MaxCorridorDepth = 3,
            PuzzleCount = 3,
        });

        // Audio
        SetAudioEffectEnabled(true);

        // Navigation
        NavRegion.BakeNavigationMesh();

        // Enemies
        EnemyController.Instance.SpawnEnemies();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        // Key items
        KeyItemInventoryController.Instance.Clear();

        // Audio
        SetAudioEffectEnabled(false);
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
}

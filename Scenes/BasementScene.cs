using System.Linq;

public partial class BasementScene : Scene
{
    private float time_scene_started;

    protected override void Initialize()
    {
        base.Initialize();
        time_scene_started = GameTime.Time;
        BasementController.Instance.DebugGenerateBasement();
        Data.Game.OnBeforeSave += OnBeforeSave;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        Data.Game.OnBeforeSave -= OnBeforeSave;
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
}

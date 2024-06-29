using Godot;

public class Seed
{
    public PlantAreaData Data { get; set; }
    public float TimeStart { get; set; }
    public float TimeEnd { get; set; }
    public Node3D SeedModel { get; set; }
    public Node3D PlantModel { get; set; }
    public IInteractable InteractablePlant => PlantModel as IInteractable;
    public IGrabbable GrabbablePlant => PlantModel as IGrabbable;
    public IInteractable InteractableSeed => SeedModel as IInteractable;
    public IGrabbable GrabbableSeed => SeedModel as IGrabbable;

    public void UpdateData()
    {
        Data.TimeLeft = TimeEnd - GameTime.Time;
    }
}
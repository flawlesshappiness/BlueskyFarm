using Godot;

public class Seed
{
    public ItemInfo PlantInfo { get; set; }
    public int GrowTicksStart { get; set; }
    public int GrowTicksEnd { get; set; }
    public Node3D SeedModel { get; set; }
    public Node3D PlantModel { get; set; }
    public IInteractable InteractablePlant => PlantModel as IInteractable;
    public IGrabbable GrabbablePlant => PlantModel as IGrabbable;
    public IInteractable InteractableSeed => SeedModel as IInteractable;
    public IGrabbable GrabbableSeed => SeedModel as IGrabbable;
}

using Godot;

public class Seed
{
    public PlantAreaData Data { get; set; }
    public float TimeStart { get; set; }
    public float TimeEnd { get; set; }
    public Node3D SeedModel { get; set; }
    public Node3D PlantModel { get; set; }
    public Item ItemPlant => PlantModel as Item;
    public Item SeedItem => SeedModel as Item;

    public void UpdateData()
    {
        Data.TimeLeft = TimeEnd - GameTime.Time;
    }
}
public class Seed
{
    public PlantAreaData Data { get; set; }
    public float TimeStart { get; set; }
    public float TimeEnd { get; set; }
    public float TimeWeed { get; set; }
    public Item SeedItem { get; set; }
    public Item PlantItem { get; set; }

    public void UpdateData()
    {
        Data.TimeLeft = TimeEnd - GameTime.Time;
        Data.TimeUntilNextWeed = TimeWeed - GameTime.Time;
    }

    public void DecreaseTimeFromWeeds()
    {
        TimeEnd -= 5f;
    }
}
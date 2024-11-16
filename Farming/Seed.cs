public class Seed
{
    public PlantAreaData Data { get; set; }
    public float TimeStart { get; set; }
    public float TimeEnd { get; set; }
    public float TimeWeed { get; set; }
    public Item SeedItem { get; set; }
    public Item PlantItem { get; set; }

    public bool IsFinished => GameTime.Time > TimeEnd;

    public void UpdateData()
    {
        if (!IsFinished)
        {
            Data.TimeLeft = TimeEnd - GameTime.Time;
            Data.TimeUntilNextWeed = TimeWeed - GameTime.Time;
        }
    }

    public void DecreaseTimeFromWeeds()
    {
        if (!IsFinished)
        {
            TimeEnd -= 5f;
        }
    }
}
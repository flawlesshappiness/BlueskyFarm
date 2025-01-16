public class PlantAreaData
{
    public string Id { get; set; }
    public string SeedInfoPath { get; set; }
    public string PlantInfoPath { get; set; }
    public float TimeLeft { get; set; }
    public int WeedCount { get; set; }
    public float TimeUntilNextWeed { get; set; }
    public bool IsWatered { get; set; }
}

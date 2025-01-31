using System.Collections.Generic;

public partial class GameSaveData
{
    public BlueprintCraftingData BlueprintCraftingData { get; set; }
    public List<BlueprintCraftedCountData> BlueprintCraftedCounts { get; set; } = new();
}

public class BlueprintCraftedCountData
{
    public string Id { get; set; }
    public int Count { get; set; }
}

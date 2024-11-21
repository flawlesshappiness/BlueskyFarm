using System.Collections.Generic;

public class BlueprintCraftingData
{
    public string Id { get; set; }
    public List<BlueprintCraftingMaterialData> Materials { get; set; } = new();
    public List<ItemData> Items { get; set; } = new();
}

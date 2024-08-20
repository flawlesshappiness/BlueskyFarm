using System.Collections.Generic;

public partial class GameSaveData
{
    public List<ItemData> MaterialProcessorInputs { get; set; } = new();
    public CraftingMaterialType MaterialProcessorCurrentType { get; set; }
}

public partial class GameSaveData
{
    public int InventorySize { get; set; } = InventoryController.INIT_INVENTORY_SIZE;
    public ItemData[] InventoryItems { get; set; } = new ItemData[InventoryController.MAX_INVENTORY_SIZE];
}

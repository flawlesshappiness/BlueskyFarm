public partial class GameSaveData
{
    public bool Flag_BasementLocked { get; set; } = true;
    public bool Flag_ForestBlockadeRemoved { get; set; } = false;
    public bool Flag_WorkshopKeyAvailable { get; set; } = false;
    public bool Flag_WorkshopDoorUnlocked { get; set; } = false;
    public bool Flag_WorkshopWellRepaired { get; set; } = false;
    public int State_BasementInventoryPuzzle { get; set; } = 0;
}

public partial class GameSaveData
{
    public bool Flag_FarmFirstTimeLoad { get; set; } = true;
    public bool Flag_BasementLocked { get; set; } = true;
    public bool Flag_ForestBlockadeRemoved { get; set; } = false;
    public bool Flag_WorkshopKeyAvailable { get; set; } = false;
    public bool Flag_WorkshopDoorUnlocked { get; set; } = false;
    public bool Flag_WorkshopWellRepaired { get; set; } = false;
    public bool Flag_FounderHutWeedsCut { get; set; } = false;
    public bool Flag_ShedRepaired { get; set; } = false;
    public bool Flag_BedRepaired { get; set; } = false;
    public bool Flag_ForestTreeHouseRepaired { get; set; } = false;
    public bool Flag_MineKilnActivated { get; set; } = false;
    public int State_BasementInventoryPuzzle { get; set; } = 0;
    public int State_FarmInventoryWeedsCut { get; set; } = 0;
}

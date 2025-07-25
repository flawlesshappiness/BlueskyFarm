public static class GameFlagIds
{
    public static GameFlagId FirstTimeLoad = new GameFlagId("first_time_load");
    public static GameFlagId FarmBasementUnlocked = new GameFlagId("farm_basement_unlocked");
    public static GameFlagId FarmShedRepaired = new GameFlagId("farm_shed_repaired");
    public static GameFlagId FarmBedRepaired = new GameFlagId("farm_bed_repaired");
    public static GameFlagId FarmWeedsCut = new GameFlagId("farm_weeds_cut");
    public static GameFlagId FarmRocksCrushed = new GameFlagId("farm_rocks_crushed");
    public static GameFlagId FarmIsUnderground = new GameFlagId("farm_is_underground");
    public static GameFlagId BasementWorkshopDoorUnlocked = new GameFlagId("basement_workshop_door_unlocked");
    public static GameFlagId BasementWellRepaired = new GameFlagId("basement_well_repaired");
    public static GameFlagId BasementLadderRepaired = new GameFlagId("basement_ladder_repaired");
    public static GameFlagId BasementLadderInventoryItemPicked = new GameFlagId("basement_ladder_inventory_item_picked");
    public static GameFlagId BasementRockCrushed = new GameFlagId("basement_rock_crushed");
    public static GameFlagId BasementRockInventoryItemPicked = new GameFlagId("basement_rock_inventory_item_picked");
    public static GameFlagId BasementRockSecretInventoryItemPicked = new GameFlagId("basement_rock_secret_inventory_item_picked");
    public static GameFlagId BasementWellVegetableOffered = new GameFlagId("basement_well_vegetable_offered");
    public static GameFlagId BasementWellBoneOffered = new GameFlagId("basement_well_bone_offered");
    public static GameFlagId BasementWellCrystalOffered = new GameFlagId("basement_well_crystal_offered");
    public static GameFlagId ForestBlockadeRemoved = new GameFlagId("forest_blockade_removed");
    public static GameFlagId ForestHutWeedsCut = new GameFlagId("forest_hut_weeds_cut");
    public static GameFlagId ForestTreeHouseRepaired = new GameFlagId("forest_tree_house_repaired");
    public static GameFlagId ForestHutInventoryItemPicked = new GameFlagId("forest_hut_inventory_item_picked");
    public static GameFlagId ForestTombstoneCrushed = new GameFlagId("forest_tombstone_crushed");
    public static GameFlagId ForestTombstoneInventoryItemPicked = new GameFlagId("forest_tombstone_inventory_item_picked");
    public static GameFlagId ForestEyeInventoryItemPicked = new GameFlagId("forest_eye_inventory_item_picked");
    public static GameFlagId SecretPuzzleClockCompleted = new GameFlagId("secret_puzzle_clock_completed");
    public static GameFlagId SecretPuzzleForestCompleted = new GameFlagId("secret_puzzle_forest_completed");
    public static GameFlagId SecretPuzzleWellCompleted = new GameFlagId("secret_puzzle_well_completed");
    public static GameFlagId SecretPuzzleFounderCompleted = new GameFlagId("secret_puzzle_founder_completed");
    public static GameFlagId ForgeMoldPlaced = new GameFlagId("forge_mold_placed");
    public static GameFlagId KilnActivated = new GameFlagId("kiln_activated");
}

public class GameFlagId
{
    public string Id { get; set; }

    public GameFlagId(string id)
    {
        Id = id;
    }

    public int Get() => GameFlags.GetFlag(Id);
    public void Set(int value) => GameFlags.SetFlag(Id, value);
    public void SetTrue() => Set(1);
    public void SetFalse() => Set(0);
    public bool Is(int value) => Get() == value;
    public bool IsTrue() => Is(1);
    public bool IsFalse() => Is(0);
}
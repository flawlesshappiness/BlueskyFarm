using Godot;
using System.Collections;
using System.Collections.Generic;

public partial class FarmScene : GameScene
{
    [Export]
    public SceneDoor Trapdoor;

    [Export]
    public ItemArea BasementUnlockArea;

    [Export]
    public Weed_Thorns Shed_Thorns;

    [Export]
    public Node3D Shed_InventoryItemPosition;

    [Export]
    public ItemInfo InventoryUpgradeItem;

    [Export]
    public InteractTeleport TeleportToUnderground;

    [Export]
    public InteractTeleport TeleportToFarm;

    [Export]
    public InteractTeleport TeleportUndergroundToOtherFarm;

    [Export]
    public InteractTeleport TeleportOtherFarmToUnderground;

    [Export]
    public CrushableRock CrushableRockToUnderground;

    public List<PlantArea> PlantAreas = new();

    public override void _Ready()
    {
        base._Ready();
        PlantAreas = this.GetNodesInChildren<PlantArea>();
    }

    protected override void Initialize()
    {
        base.Initialize();

        InitializeView();
        InitializeTrapdoor();
        InitializeInventoryWeeds();
        InitializeShedInventoryItem();
        InitializePlayerInventory();
        InitializeAmbience();
        InitializeEnvironment();
        InitializeCrushableRock();
        InitializeTeleports();

        GameFlagIds.FirstTimeLoad.Set(1);
    }

    private void InitializeView()
    {
        var view = View.Get<GameView>();
        view.Minimap.Clear();
        view.Minimap.Hide();

        ScreenEffects.Instance.Clear();
    }

    private void InitializeTrapdoor()
    {
        BasementUnlockArea.OnItemEntered += BasementUnlockArea_ItemEntered;
        Trapdoor.SetLocked(GameFlagIds.FarmBasementUnlocked.IsFalse());

        Trapdoor.OnSceneChange += () =>
        {
            Data.Game.PlayerPositionNode = "BasementStart";
        };
    }

    private void InitializeInventoryWeeds()
    {
        Shed_Thorns.OnWeedCut += () =>
        {
            GameFlagIds.FarmWeedsCut.SetTrue();
        };

        Shed_Thorns.SetCut(GameFlagIds.FarmWeedsCut.IsTrue());
    }

    private void InitializeShedInventoryItem()
    {
        if (GameFlagIds.FirstTimeLoad.Is(1)) return;

        var item = ItemController.Instance.CreateItem(InventoryUpgradeItem);
        item.GlobalPosition = Shed_InventoryItemPosition.GlobalPosition;
    }

    private void InitializePlayerInventory()
    {
        InventoryController.Instance.RemoveNonFarmItems();
    }

    private void InitializeAmbience()
    {
        var is_underground = GameFlagIds.FarmIsUnderground.IsTrue();
        var type = is_underground ? AreaNames.Basement : AreaNames.Farm;
        AmbienceController.Instance.StartAmbienceImmediate(type);
    }

    private void InitializeEnvironment()
    {
        var is_underground = GameFlagIds.FarmIsUnderground.IsTrue();
        var type = is_underground ? AreaNameType.Basement : AreaNameType.Farm;
        EnvironmentController.Instance.SetEnvironment(type);
    }

    private void InitializeCrushableRock()
    {
        var crushed = GameFlagIds.BasementRockCrushed.IsTrue();
        CrushableRockToUnderground.SetEnabled(!crushed);
        TeleportToUnderground.SetEnabled(crushed);

        CrushableRockToUnderground.OnCrushed += () =>
        {
            GameFlagIds.BasementRockCrushed.SetTrue();
            Data.Game.Save();

            CrushableRockToUnderground.Disable();
            TeleportToUnderground.Enable();
        };
    }

    private void InitializeTeleports()
    {
        TeleportToFarm.OnTouched += Touched_TeleportToFarm;
        TeleportUndergroundToOtherFarm.OnTouched += Touched_TeleportToFarm;
        TeleportToUnderground.OnTouched += Touched_TeleportToUnderground;
        TeleportOtherFarmToUnderground.OnTouched += Touched_TeleportToUnderground;
    }

    protected override void BeforeSave()
    {
        base.BeforeSave();

        Player.Instance.UpdateData();
        InventoryController.Instance.UpdateData();
        ItemController.Instance.UpdateData();
        Debug.WriteLogsToPersistentData();
        UpdateData_PlantArea();
    }

    private void UpdateData_PlantArea()
    {
        foreach (var area in PlantAreas)
        {
            area.UpdateData();
        }
    }

    private void BasementUnlockArea_ItemEntered(Item item)
    {
        item.IsBeingHandled = true;
        GameFlagIds.FarmBasementUnlocked.SetTrue();

        Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            SoundController.Instance.Play("sfx_basement_unlock", Trapdoor.GlobalPosition);
            yield return item.AnimateDisappearAndQueueFree();
            Trapdoor.SetLocked(false);
        }
    }

    private void Touched_TeleportToUnderground()
    {
        GameFlagIds.FarmIsUnderground.SetTrue();
    }

    private void Touched_TeleportToFarm()
    {
        GameFlagIds.FarmIsUnderground.SetFalse();
    }
}

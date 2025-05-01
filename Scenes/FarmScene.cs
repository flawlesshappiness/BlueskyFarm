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
        AmbienceController.Instance.StartAmbienceImmediate(AreaNames.Farm);
    }

    private void InitializeEnvironment()
    {
        EnvironmentController.Instance.SetEnvironment(AreaNameType.Farm);
    }

    protected override void BeforeSave()
    {
        base.BeforeSave();

        Player.Instance.UpdateData();
        InventoryController.Instance.UpdateData();
        ItemController.Instance.UpdateData();
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

        Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            SoundController.Instance.Play("sfx_basement_unlock", Trapdoor.GlobalPosition);
            yield return item.AnimateDisappearAndQueueFree();
            Trapdoor.SetLocked(false);
            GameFlagIds.FarmBasementUnlocked.SetTrue();
        }
    }
}

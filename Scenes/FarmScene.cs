using Godot;
using System.Collections;
using System.Collections.Generic;

public partial class FarmScene : GameScene
{
    [NodeName]
    public SceneDoor Trapdoor;

    [NodeName]
    public ItemArea BasementUnlockArea;

    [NodeName]
    public Weed_Thorns Shed_Thorns;

    [NodeName]
    public Node3D Shed_InventoryItemPosition;

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

        Data.Game.Flag_FarmFirstTimeLoad = false;
        Data.Game.Save();
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
        Trapdoor.Locked = Data.Game.Flag_BasementLocked;
    }

    private void InitializeInventoryWeeds()
    {
        Shed_Thorns.OnWeedCut += () =>
        {
            Data.Game.State_FarmInventoryWeedsCut = 1;
        };

        Shed_Thorns.SetCut(Data.Game.State_FarmInventoryWeedsCut != 0);
    }

    private void InitializeShedInventoryItem()
    {
        if (!Data.Game.Flag_FarmFirstTimeLoad) return;

        var item = ItemController.Instance.CreateItem("InventoryUpgrade");
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
            Trapdoor.Locked = false;
            Data.Game.Flag_BasementLocked = false;
        }
    }
}

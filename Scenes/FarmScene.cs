using Godot;
using System.Collections;

public partial class FarmScene : GameScene
{
    [NodeName]
    public SceneDoor Trapdoor;

    [NodeName]
    public Area3D BasementUnlockArea;

    protected override void Initialize()
    {
        base.Initialize();

        BasementUnlockArea.BodyEntered += go => CallDeferred(nameof(BasementUnlockArea_BodyEntered), go);

        var view = View.Get<GameView>();
        view.Minimap.Clear();
        view.Minimap.Hide();

        ScreenEffects.Instance.Clear();

        LoadGame();
    }

    private void LoadGame()
    {
        Trapdoor.Locked = Data.Game.Flag_BasementLocked;
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
        var areas = Scene.Current.GetNodesInChildren<PlantArea>();
        foreach (var area in areas)
        {
            area.UpdateData();
        }
    }

    private void BasementUnlockArea_BodyEntered(GodotObject go)
    {
        if (!IsInstanceValid(go)) return;

        var node = go as Node3D;
        if (node == null) return;

        var item = node.GetNodeInParents<Item>();
        if (item == null) return;

        if (item.Data.CustomId == "basement_key")
        {
            item.IsBeingHandled = true;
            Coroutine.Start(Cr);
        }

        IEnumerator Cr()
        {
            SoundController.Instance.Play("sfx_basement_unlock", Trapdoor.GlobalPosition);
            yield return item.AnimateDisappearAndQueueFree();
            Trapdoor.Locked = false;
            Data.Game.Flag_BasementLocked = false;
        }
    }
}

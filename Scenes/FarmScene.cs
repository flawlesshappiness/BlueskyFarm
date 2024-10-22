public partial class FarmScene : GameScene
{
    protected override void Initialize()
    {
        base.Initialize();

        var view = View.Get<GameView>();
        view.Minimap.Clear();
        view.Minimap.Hide();

        Data.Game.Save();
    }

    protected override void BeforeSave()
    {
        base.BeforeSave();

        FirstPersonController.Instance.UpdateData();
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
}

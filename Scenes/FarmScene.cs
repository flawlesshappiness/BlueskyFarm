public partial class FarmScene : Scene
{
    protected override void Initialize()
    {
        base.Initialize();
        Data.Game.OnBeforeSave += OnBeforeSave;
        Data.Game.Save();

        var view = View.Get<GameView>();
        view.Minimap.Clear();
        view.Minimap.Hide();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        Data.Game.OnBeforeSave -= OnBeforeSave;
    }

    private void OnBeforeSave()
    {
        FirstPersonController.Instance.UpdateData();
        InventoryController.Instance.UpdateData();
        ItemController.Instance.UpdateData();
    }
}

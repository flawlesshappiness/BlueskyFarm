public partial class FarmScene : Scene
{
    protected override void Initialize()
    {
        base.Initialize();
        Data.Game.OnBeforeSave += OnBeforeSave;
        Data.Game.Save();
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

public partial class FarmScene : Scene
{
    protected override void Initialize()
    {
        base.Initialize();
        Data.Game.OnBeforeSave += OnBeforeSave;
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
    }
}

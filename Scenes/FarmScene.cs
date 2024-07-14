public partial class FarmScene : Scene
{
    protected override void Initialize()
    {
        base.Initialize();

        var view = View.Get<GameView>();
        view.Minimap.Clear();
        view.Minimap.Hide();

        if (Data.Game.FirstTimeBoot)
        {
            CurrencyController.Instance.AddValue(CurrencyType.Money, 10);
            Data.Game.FirstTimeBoot = false;
        }

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

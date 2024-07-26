public partial class FarmScene : GameScene
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

        Data.Game.Save();
    }

    protected override void BeforeSave()
    {
        base.BeforeSave();

        FirstPersonController.Instance.UpdateData();
        InventoryController.Instance.UpdateData();
        ItemController.Instance.UpdateData();
    }
}

using Godot;

public partial class FarmScene : GameScene
{
    [NodeName]
    public Node3D OtherFarm_Tree;

    protected override void Initialize()
    {
        base.Initialize();

        InitializeOtherFarmObjects();

        var view = View.Get<GameView>();
        view.Minimap.Clear();
        view.Minimap.Hide();

        if (Data.Game.FirstTimeBoot)
        {
            CurrencyController.Instance.AddValue(CurrencyType.Money, 10);
            Data.Game.FirstTimeBoot = false;
        }

        KeyItemController.Instance.ClearTemporary();

        Data.Game.Save();
    }

    private void InitializeOtherFarmObjects()
    {
        OtherFarm_Tree.Hide();

        var rng = new RandomNumberGenerator();
        var farm_size = 32;
        var size = 7;
        for (int z = -size; z < size; z++)
        {
            for (int x = -size; x < size; x++)
            {
                if (x == 0 && z == 0) continue;

                var size_add = new Vector3(x, 0, z).Length() * 0.5f;

                var tree = OtherFarm_Tree.Duplicate() as Node3D;
                tree.SetParent(OtherFarm_Tree.GetParent());
                tree.Show();
                tree.GlobalPosition = new Vector3(farm_size * x, 0, farm_size * z);
                tree.GlobalRotation = new Vector3(0, rng.RandfRange(-15, 15), 0);
                tree.Scale = Vector3.One * (rng.RandfRange(0.7f, 1.3f));
            }
        }
    }

    protected override void BeforeSave()
    {
        base.BeforeSave();

        FirstPersonController.Instance.UpdateData();
        InventoryController.Instance.UpdateData();
        ItemController.Instance.UpdateData();
    }
}

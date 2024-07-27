using Godot;

public partial class ChestPuzzleChest : Touchable
{
    [Export]
    public string KeyItemId;

    [NodeName]
    public Node3D Open;

    [NodeName]
    public Node3D Closed;

    [NodeName]
    public Node3D ItemSpawnPosition;

    private bool _open = false;

    public override void _Ready()
    {
        base._Ready();
        Hide();
        this.SetCollisionEnabled(false);

        OnTouched += Touched;
    }

    private void Touched()
    {
        if (_open) return;
        if (string.IsNullOrEmpty(KeyItemId)) return;
        if (!KeyItemInventoryController.Instance.HasItem(KeyItemId)) return;

        KeyItemInventoryController.Instance.Remove(KeyItemId);
        Unlock();
    }

    public void Unlock()
    {
        SoundController.Instance.Play("sfx_chest_open", new SoundSettings3D
        {
            Bus = SoundBus.SFX,
            Position = GlobalPosition,
            Volume = -20
        });

        Closed.Hide();
        Open.Show();
        _open = true;
        SetCollisionMode(InteractCollisionMode.None);
        SetCollisionLayerValue(1, true);
        SpawnItems();
    }

    public void Activate()
    {
        Show();
        this.SetCollisionEnabled(true);
        Open.Hide();
        Closed.Show();
    }

    private void SpawnItems()
    {
        for (int i = 0; i < 3; i++)
        {
            var seed = CreateSeed();
            seed.GlobalPosition = ItemSpawnPosition.GlobalPosition;
            seed.LinearVelocity = ItemSpawnPosition.GlobalBasis * Vector3.Back * 3;
        }
    }

    private Item CreateSeed()
    {
        var seed_item_path = ItemController.Instance.Collection.Seed;
        var seed_item = ItemController.Instance.CreateItem(seed_item_path);
        seed_item.Data.PlantInfoPath = ItemController.Instance.Collection.Radish;
        return seed_item;
    }
}

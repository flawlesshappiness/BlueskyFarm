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
        if (CanUnlock())
        {
            KeyItemController.Instance.Remove(KeyItemId);
            Unlock();
        }
        else
        {
            Locked();
        }
    }

    private bool CanUnlock()
    {
        if (_open) return false;
        if (string.IsNullOrEmpty(KeyItemId)) return false;
        if (!KeyItemController.Instance.HasItem(KeyItemId)) return false;

        return true;
    }

    private void Locked()
    {
        SoundController.Instance.Play("sfx_chest_locked", new SoundSettings3D
        {
            Bus = SoundBus.SFX,
            Position = GlobalPosition,
            Volume = 0
        });

        View.Get<TextView>().DisplayText("The chest is locked.");
    }

    public void Unlock()
    {
        SoundController.Instance.Play("sfx_chest_open", new SoundSettings3D
        {
            Bus = SoundBus.SFX,
            Position = GlobalPosition,
            Volume = 0
        });

        Closed.Hide();
        Open.Show();
        _open = true;
        SetCollisionWithAll();
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
            seed.RigidBody.LinearVelocity = ItemSpawnPosition.GlobalBasis * Vector3.Back * 3;
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

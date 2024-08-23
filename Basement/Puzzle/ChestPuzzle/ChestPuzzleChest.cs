using Godot;
using System.Collections;

public partial class ChestPuzzleChest : Touchable
{
    [Export]
    public string CustomId;

    [NodeName]
    public Node3D Open;

    [NodeName]
    public Node3D Closed;

    [NodeName]
    public Node3D ItemSpawnPosition;

    [NodeName]
    public Area3D UnlockArea;

    private bool _open = false;

    public override void _Ready()
    {
        base._Ready();
        Hide();
        this.SetCollisionEnabled(false);

        OnTouched += Touched;
        UnlockArea.BodyEntered += n => CallDeferred(nameof(BodyEntered), n);
    }

    private void BodyEntered(GodotObject obj)
    {
        var n = obj as Node3D;
        var key = n.GetNodeInParents<PuzzleKey>();
        if (key == null) return;

        var valid_key = key.Data.CustomId == CustomId;

        if (CanUnlock() && valid_key)
        {
            AnimateUnlock(key);
        }
    }

    private void Touched()
    {
        if (!_open)
        {
            Locked();
        }
    }

    private bool CanUnlock()
    {
        if (_open) return false;
        if (string.IsNullOrEmpty(CustomId)) return false;
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

    private void AnimateUnlock(PuzzleKey key)
    {
        _open = true;
        Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            SoundController.Instance.Play("sfx_puzzle_solved", new SoundSettings3D
            {
                Bus = SoundBus.SFX,
                Position = key.GlobalPosition,
            });

            yield return key.AnimateGlowThenDisappear();
            Unlock();
        }
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
        SetCollisionLayer(player: true);
        SetCollisionMask();
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
        for (int i = 0; i < 1; i++)
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

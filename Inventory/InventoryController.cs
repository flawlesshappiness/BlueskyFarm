using Godot;
using System;
using System.Collections;
using System.Linq;

public partial class InventoryController : SingletonController
{
    public override string Directory => "Inventory";
    public static InventoryController Instance => Singleton.Get<InventoryController>();
    public ItemData[] CurrentInventoryItems { get; private set; } = new ItemData[MAX_INVENTORY_SIZE];
    private FirstPersonController Player => FirstPersonController.Instance;

    public const int INIT_INVENTORY_SIZE = 5;
    public const int MAX_INVENTORY_SIZE = 15;

    public event Action<int> OnItemAdded;
    public event Action<int> OnItemRemoved;

    protected override void Initialize()
    {
        base.Initialize();
        CurrentInventoryItems = Data.Game.InventoryItems.ToArray();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        Process_PickUp();
        Process_DropInventory();
    }

    private void Process_PickUp()
    {
        if (PlayerInput.PickUp.Pressed)
        {
            var item = Player.Interact?.CurrentInteractable as Item;
            PickUpItem(item);
        }
    }

    private void Process_DropInventory()
    {
        if (PlayerInput.DropInventory.Pressed)
        {
            DropInventory();
        }
    }

    public void ClearInventory()
    {
        Data.Game.InventoryItems = new ItemData[MAX_INVENTORY_SIZE];
        ClearCurrentInventory();
    }

    public void ClearCurrentInventory()
    {
        CurrentInventoryItems = new ItemData[MAX_INVENTORY_SIZE];
    }

    public void UpdateData()
    {
        Data.Game.InventoryItems = CurrentInventoryItems.ToArray();
    }

    public bool HasItem(ItemData item)
    {
        return CurrentInventoryItems.Any(x => x != null && x.Id == item.Id);
    }

    public int GetItemIndex(ItemData data) => CurrentInventoryItems.ToList().IndexOf(data);
    public ItemData GetItem(int i) => IsValidItemIndex(i) ? CurrentInventoryItems[i] : null;
    public bool IsValidItemIndex(int i) => i >= 0 && i < Data.Game.InventorySize && i < CurrentInventoryItems.Length;
    public bool HasFreeSpace() => GetFreeIndex() > -1;

    private int GetFreeIndex()
    {
        for (int i = 0; i < Data.Game.InventorySize; i++)
        {
            if (CurrentInventoryItems[i] == null) return i;
        }

        return -1;
    }

    public void Add(Item item)
    {
        if (HasItem(item.Data)) return;

        Debug.TraceMethod(item.Data.InfoPath);
        Debug.Indent++;

        var i = GetFreeIndex();
        if (i == -1) return;

        CurrentInventoryItems[i] = item.Data;
        OnItemAdded?.Invoke(i);

        Debug.Indent--;
    }

    public void PickUpItem(Item item)
    {
        if (!IsInstanceValid(item)) return;
        if (!HasFreeSpace()) return;
        if (!item.Info.CanPickUp) return;

        item.AddToInventory();
        ItemController.Instance.UntrackItem(item);
        PlayPickupSoundFx();

        Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            var start_position = item.GlobalPosition;
            var start_scale = item.Scale;
            var end_scale = start_scale * 0.25f;

            item.IsBeingHandled = true;
            item.SetCollisionEnabled(false);
            item.RigidBody.GravityScale = 0;
            item.RigidBody.LinearVelocity = Vector3.Zero;

            yield return LerpEnumerator.Lerp01(0.1f, f =>
            {
                item.RigidBody.GlobalPosition = start_position.Lerp(Player.Mid.GlobalPosition, f);
                item.RigidBody.Scale = start_scale.Lerp(end_scale, f);
            });

            item.SetEnabled(false);
            item.QueueFree();
        }
    }

    public void DropInventory()
    {
        Debug.TraceMethod();

        Coroutine.Start(DropAllCr);
        IEnumerator DropAllCr()
        {
            for (int i = 0; i < CurrentInventoryItems.Length; i++)
            {
                DropItem(i);
                yield return new WaitForSeconds(0.05f);
            }

            ClearCurrentInventory();
            Data.Game.Save();
        }
    }

    private void DropItem(int i)
    {
        var item = CurrentInventoryItems[i];
        if (item == null) return;

        DropItem(item);
        OnItemRemoved?.Invoke(i);
    }

    private void DropItem(ItemData data)
    {
        if (data == null) return;

        var dir = Player.Camera.GlobalBasis * Vector3.Forward;
        var vel = Player.Velocity;
        var item = ItemController.Instance.CreateItemFromData(data);
        item.RigidBody.GlobalPosition = Player.Mid.GlobalPosition;
        item.RigidBody.LinearVelocity = vel + dir * 5;

        PlayPickupSoundFx();
    }

    private void PlayPickupSoundFx()
    {
        SoundController.Instance.Play("sfx_pickup", new SoundSettings
        {
            PitchMin = 0.8f,
            PitchMax = 1.2f
        });
    }
}

using Godot;
using System;
using System.Collections;
using System.Linq;

public partial class InventoryController : SingletonController
{
    public override string Directory => "Inventory";
    public static InventoryController Instance => Singleton.Get<InventoryController>();
    private FirstPersonController Player => FirstPersonController.Instance;
    public ItemData[] CurrentInventoryItems { get; private set; } = new ItemData[MAX_INVENTORY_SIZE];
    public int SelectedIndex { get; private set; }

    public const int INIT_INVENTORY_SIZE = 5;
    public const int MAX_INVENTORY_SIZE = 15;

    public event Action<int> OnItemAdded;
    public event Action<int> OnItemRemoved;
    public event Action<int> OnSelectedItemChanged;
    public event Action<int> OnSelectedItemUsed;

    private bool _dropping_inventory;
    private float _time_select_index;
    private float _time_drop_inventory;

    protected override void Initialize()
    {
        base.Initialize();
        LoadData();
        Input_SetInventoryIndex(0);
    }

    private void LoadData()
    {
        CurrentInventoryItems = Data.Game.InventoryItems.ToList().ToArray();
    }

    public void UpdateData()
    {
        Data.Game.InventoryItems = CurrentInventoryItems.ToList().ToArray();
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);

        if (PlayerInput.NextInventorySlot.Pressed)
        {
            Input_SetInventoryIndex(SelectedIndex + 1);
        }
        else if (PlayerInput.PreviousInventorySlot.Pressed)
        {
            Input_SetInventoryIndex(SelectedIndex - 1);
        }

        if (PlayerInput.DropInventory.Pressed)
        {
            Input_DropInventoryPressed();
        }
        else if (PlayerInput.DropInventory.Held)
        {
            Input_DropInventoryHeld();
        }
        else if (PlayerInput.DropInventory.Released)
        {
            Input_DropInventoryReleased();
        }

        if (PlayerInput.PickUp.Pressed)
        {
            Input_PickUpPressed();
        }

        if (PlayerInput.UseItem.Pressed)
        {
            Input_UseItem();
        }
    }

    private void Input_UseItem()
    {
        OnSelectedItemUsed?.Invoke(SelectedIndex);
    }

    private void Input_SetInventoryIndex(int i)
    {
        if (GameTime.Time < _time_select_index) return;
        _time_select_index = GameTime.Time + 0.01f;

        SelectedIndex = Mathf.Clamp(i, 0, Data.Game.InventorySize - 1);
        OnSelectedItemChanged?.Invoke(SelectedIndex);
    }

    private void Input_DropInventoryPressed()
    {
        _time_drop_inventory = GameTime.Time + 0.5f;
    }

    private void Input_DropInventoryHeld()
    {
        if (GameTime.Time < _time_drop_inventory) return;
        DropInventory();
    }

    private void Input_DropInventoryReleased()
    {
        if (_dropping_inventory) return;

        DropItem(SelectedIndex);
    }

    private void Input_PickUpPressed()
    {
        var item = Player.Interact?.CurrentInteractable as Item;
        PickUpItem(item);
    }

    public void ClearInventory()
    {
        Data.Game.InventoryItems = new ItemData[MAX_INVENTORY_SIZE];
        ClearCurrentInventory();
    }

    public void ClearCurrentInventory()
    {
        for (int i = 0; i < MAX_INVENTORY_SIZE; i++)
        {
            RemoveItem(i);
        }
    }

    public bool HasItem(ItemData item)
    {
        return CurrentInventoryItems.Any(x => x != null && x.Id == item.Id);
    }

    public int GetItemIndex(ItemData data) => CurrentInventoryItems.ToList().IndexOf(data);
    public ItemData GetItem(int i) => IsValidItemIndex(i) ? CurrentInventoryItems[i] : null;
    public bool IsValidItemIndex(int i) => i >= 0 && i < Data.Game.InventorySize && i < CurrentInventoryItems.Length;
    public bool HasFreeSpace() => GetFreeIndex() > -1;
    public bool IsInventoryEmpty() => !CurrentInventoryItems.Any(x => x != null);

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

        item.UpdateData();
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
        if (IsInventoryEmpty()) return;
        if (_dropping_inventory) return;

        Debug.TraceMethod();

        _dropping_inventory = true;
        var items = CurrentInventoryItems.ToList().ToArray();
        ClearCurrentInventory();
        Data.Game.Save();

        Coroutine.Start(DropAllCr);
        IEnumerator DropAllCr()
        {
            for (int i = 0; i < items.Length; i++)
            {
                var item = items[i];
                if (item == null) continue;

                DropItem(item);
                OnItemRemoved?.Invoke(i);
                yield return new WaitForSeconds(0.05f);
            }

            _dropping_inventory = false;
        }
    }

    private void DropItem(int i)
    {
        var item = CurrentInventoryItems[i];
        CurrentInventoryItems[i] = null;
        if (item == null) return;

        DropItem(item);
        OnItemRemoved?.Invoke(i);
    }

    private void RemoveItem(int i)
    {
        var item = CurrentInventoryItems[i];
        CurrentInventoryItems[i] = null;
        if (item == null) return;

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

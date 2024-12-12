using Godot;
using System;
using System.Collections;
using System.Linq;

public partial class InventoryController : SingletonController
{
    public override string Directory => "Inventory";
    public static InventoryController Instance => Singleton.Get<InventoryController>();
    public ItemData[] CurrentInventoryItems { get; private set; } = new ItemData[MAX_INVENTORY_SIZE];
    public int SelectedIndex { get; private set; }

    public MultiLock InventoryLock = new();

    public const int INIT_INVENTORY_SIZE = 2;
    public const int MAX_INVENTORY_SIZE = 10;

    public event Action<int> OnItemAdded;
    public event Action<int> OnItemRemoved;
    public event Action<int> OnSelectedItemChanged;
    public event Action<int> OnSelectedItemUsed;
    public event Action OnInventorySizeChanged;

    private bool _dropping_inventory;
    private float _time_select_index;
    private float _time_drop_inventory;

    protected override void Initialize()
    {
        base.Initialize();
        RegisterDebugActions();
        LoadData();
        Input_SetInventoryIndex(0);
    }

    private void RegisterDebugActions()
    {
        var category = "INVENTORY";

        Debug.RegisterAction(new DebugAction
        {
            Category = category,
            Text = "Replenish uses",
            Action = ReplenishItem
        });

        Debug.RegisterAction(new DebugAction
        {
            Category = category,
            Text = "Deplete uses",
            Action = DepleteItem
        });

        Debug.RegisterAction(new DebugAction
        {
            Category = category,
            Text = "Set custom id",
            Action = SetCustomId
        });

        void ReplenishItem(DebugView view)
        {
            var data = GetSelectedItem();
            if (data == null) return;
            var info = ItemController.Instance.GetInfoFromPath(data.Info);
            data.Uses = info.Uses;
        }

        void DepleteItem(DebugView view)
        {
            var data = GetSelectedItem();
            if (data == null) return;
            data.Uses = 0;
        }

        Debug.RegisterAction(new DebugAction
        {
            Category = category,
            Text = "Increase inventory size",
            Action = v => { IncreaseInventorySize(1); }
        });

        Debug.RegisterAction(new DebugAction
        {
            Category = category,
            Text = "Decrease inventory size",
            Action = v => { DecreaseInventorySize(1); }
        });

        void SetCustomId(DebugView view)
        {
            view.PopupStringInput("Custom id", id =>
            {
                Input(id);
            });

            void Input(string custom_id)
            {
                var data = GetSelectedItem();
                if (data == null) return;
                data.CustomId = custom_id;
            }
        }
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

        if (InventoryLock.IsLocked) return;

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
        var item = Player.Instance.Interact?.CurrentInteractable as Item;
        if (!IsInstanceValid(item)) return;

        item.PickUp();
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
    public ItemData GetSelectedItem() => GetItem(SelectedIndex);
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

    public void Add(ItemInfo info)
    {
        if (info == null) return;
        var data = ItemController.Instance.CreateItemData(info);
        Add(data);
    }

    public void Add(ItemData data)
    {
        if (data == null) return;
        if (HasItem(data)) return;

        Debug.TraceMethod(data.Info);
        Debug.Indent++;

        var i = GetFreeIndex();
        if (i == -1) return;

        CurrentInventoryItems[i] = data;
        OnItemAdded?.Invoke(i);

        Debug.Indent--;
    }

    public void PickUpItem(Item item)
    {
        if (!IsInstanceValid(item)) return;
        if (!HasFreeSpace()) return;
        if (!item.Info.CanPickUp) return;

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

            PlayPickupSoundFx();

            yield return LerpEnumerator.Lerp01(0.1f, f =>
            {
                item.RigidBody.GlobalPosition = start_position.Lerp(Player.Instance.MidPosition, f);
                item.RigidBody.Scale = start_scale.Lerp(end_scale, f);
            });

            Add(item.Data);

            ItemController.Instance.UntrackItem(item);
            item.Disable();
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

        var dir = Player.Instance.CameraTarget.GlobalBasis * Vector3.Forward;
        var vel = Player.Instance.Velocity;
        var item = ItemController.Instance.CreateItemFromData(data);
        item.RigidBody.GlobalPosition = Player.Instance.MidPosition;
        item.RigidBody.LinearVelocity = vel + dir * 5;

        PlayPickupSoundFx();
    }

    private void PlayPickupSoundFx()
    {
        SoundController.Instance.Play("sfx_pickup");
    }

    public void IncreaseInventorySize(int amount = 1)
    {
        AdjustInventorySize(Mathf.Max(1, amount));
    }

    public void DecreaseInventorySize(int amount = 1)
    {
        AdjustInventorySize(-Mathf.Max(1, amount));
    }

    private void AdjustInventorySize(int amount)
    {
        Data.Game.InventorySize = Mathf.Clamp(Data.Game.InventorySize + amount, 1, MAX_INVENTORY_SIZE);
        OnInventorySizeChanged?.Invoke();
    }
}

using Godot;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class InventoryController : SingletonController
{
    public override string Directory => "Inventory";
    public static InventoryController Instance => Singleton.Get<InventoryController>();

    public List<ItemData> CurrentInventoryItems { get; set; } = new();

    private FirstPersonController Player => FirstPersonController.Instance;

    protected override void Initialize()
    {
        base.Initialize();
        CurrentInventoryItems = Data.Game.InventoryItems.ToList();
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

    public void UpdateData()
    {
        Data.Game.InventoryItems = CurrentInventoryItems.ToList();
    }

    public bool HasItem(ItemData item)
    {
        return CurrentInventoryItems.Any(x => x.Id == item.Id);
    }

    public void Add(Item item)
    {
        if (HasItem(item.Data)) return;

        Debug.TraceMethod(item.Data.InfoPath);
        Debug.Indent++;

        if (item.Info.IsMoney)
        {
            CurrencyController.Instance.AddValue(CurrencyType.Money, 1);
        }
        else
        {
            CurrentInventoryItems.Add(item.Data);
        }

        Debug.Indent--;
    }

    public void PickUpItem(Item item)
    {
        if (item == null) return;
        if (!item.Info.CanPickUp) return;

        Add(item);
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
            item.GravityScale = 0;
            item.LinearVelocity = Vector3.Zero;

            yield return LerpEnumerator.Lerp01(0.1f, f =>
            {
                item.GlobalPosition = start_position.Lerp(Player.Mid.GlobalPosition, f);
                item.Scale = start_scale.Lerp(end_scale, f);
            });

            item.SetEnabled(false);
            item.QueueFree();
        }
    }

    public void DropInventory()
    {
        Debug.TraceMethod();
        Debug.Indent++;

        var items = CurrentInventoryItems.ToList();
        CurrentInventoryItems.Clear();
        Data.Game.Save();

        Coroutine.Start(DropAllCr);

        Debug.Indent--;

        IEnumerator DropAllCr()
        {
            foreach (var inv_item in items)
            {
                var dir = Player.Camera.GlobalBasis * Vector3.Forward;
                var vel = Player.Velocity;
                var item = ItemController.Instance.CreateItemFromData(inv_item);
                item.GlobalPosition = Player.Mid.GlobalPosition;
                item.LinearVelocity = vel + dir * 5;

                PlayPickupSoundFx();

                yield return new WaitForSeconds(0.05f);
            }
        }
    }

    private void PlayPickupSoundFx()
    {
        SoundController.Instance.Play(SoundName.Pickup, new SoundSettings
        {
            PitchMin = 0.8f,
            PitchMax = 1.2f
        });
    }
}

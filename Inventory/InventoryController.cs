using Godot;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class InventoryController : SingletonController
{
    public override string Directory => "Inventory";
    public static InventoryController Instance => Singleton.Get<InventoryController>();

    public List<InventoryItem> CurrentInventoryItems { get; set; } = new();

    private FirstPersonController Player => FirstPersonController.Instance;

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

    public void CommitToData()
    {
        Data.Game.InventoryItems = CurrentInventoryItems;
        Data.Game.Save();
    }

    public InventoryItem Get(ItemInfo info)
    {
        var item = CurrentInventoryItems.FirstOrDefault(x => x.Info.Path == info.Path);

        if (item == null)
        {
            item = new InventoryItem { Info = info };
            CurrentInventoryItems.Add(item);
        }

        return item;
    }

    public void Add(ItemInfo info, int count = 1)
    {
        Debug.LogMethod($"{info.Path}: {count}");
        Debug.Indent++;

        var item = Get(info);
        item.Count += count;

        Debug.Log("Current count: " + item.Count);

        Debug.Indent--;
    }

    public void Remove(ItemInfo info, int count = 1)
    {
        Debug.LogMethod($"{info.Path}: {count}");
        Debug.Indent++;

        var item = Get(info);
        item.Count -= count;

        Debug.Indent--;
    }

    public void PickUpItem(Item item)
    {
        if (item == null) return;

        Add(item.Info);
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
        Debug.LogMethod();
        Debug.Indent++;

        var items = CurrentInventoryItems.ToList();
        CurrentInventoryItems.Clear();
        Coroutine.Start(DropAllCr);

        Debug.Indent--;

        IEnumerator DropAllCr()
        {
            foreach (var inv_item in items)
            {
                for (int i = 0; i < inv_item.Count; i++)
                {
                    var dir = Player.Camera.GlobalBasis * Vector3.Forward;
                    var vel = Player.Velocity;
                    var item = ItemController.Instance.CreateItem(inv_item.Info);
                    item.GlobalPosition = Player.Mid.GlobalPosition;
                    item.LinearVelocity = vel + dir * 5;

                    PlayPickupSoundFx();

                    yield return new WaitForSeconds(0.05f);
                }
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

using Godot;
using System;

public partial class ForestTreeHousePlankPosition : Node3D
{
    [Export]
    public ItemArea ItemArea;

    [Export]
    public Node3D Plank;

    [Export]
    public SoundInfo SfxAttach;

    public bool Repaired { get; private set; }

    public event Action OnRepaired;

    public override void _Ready()
    {
        base._Ready();
        ItemArea.OnItemEntered += ItemEntered;
    }

    private void ItemEntered(Item item)
    {
        ItemController.Instance.UntrackItem(item);
        item.QueueFree();

        SoundController.Instance.Play(SfxAttach, Plank.GlobalPosition);

        SetRepaired();

        OnRepaired?.Invoke();
    }

    public void SetRepaired(bool repaired)
    {
        if (repaired)
        {
            SetRepaired();
        }
        else
        {
            SetNotRepaired();
        }
    }

    public void SetRepaired()
    {
        Repaired = true;

        Plank.Enable();
        ItemArea.Disable();
    }

    public void SetNotRepaired()
    {
        Repaired = false;

        Plank.Disable();
        ItemArea.Enable();
    }
}

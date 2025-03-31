using Godot;
using System;

public partial class RootCore : Node3D
{
    [Export]
    public ItemArea ItemArea;

    public event Action OnSwordEntered;

    public override void _Ready()
    {
        base._Ready();
        ItemArea.OnItemEntered += ItemEntered_Sword;
    }

    private void ItemEntered_Sword(Item item)
    {
        item.QueueFree();
        OnSwordEntered?.Invoke();
    }
}

using Godot;
using System;
using System.Collections;

public partial class RootCore : Node3D
{
    [Export]
    public ItemArea ItemArea_Sword;

    [Export]
    public ItemArea ItemArea_Potion;

    [Export]
    public AnimationPlayer AnimationPlayer_Sword;

    public event Action OnSwordEntered;
    public event Action OnPotionEntered;

    public override void _Ready()
    {
        base._Ready();
        ItemArea_Sword.OnItemEntered += ItemEntered_Sword;
        ItemArea_Potion.OnItemEntered += ItemEntered_Potion;
    }

    private void ItemEntered_Sword(Item item)
    {
        item.QueueFree();
        OnSwordEntered?.Invoke();
    }

    private void ItemEntered_Potion(Item item)
    {
        item.QueueFree();
        OnPotionEntered?.Invoke();
    }

    public Coroutine AnimateSwordIntoCore()
    {
        return this.StartCoroutine(Cr);
        IEnumerator Cr()
        {
            yield return AnimationPlayer_Sword.PlayAndWaitForAnimation("sword_insert");
        }
    }
}

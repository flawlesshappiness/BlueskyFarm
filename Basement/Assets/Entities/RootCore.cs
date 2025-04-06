using Godot;
using System;
using System.Collections;

public partial class RootCore : Node3D
{
    [Export]
    public ItemArea ItemArea;

    [Export]
    public AnimationPlayer AnimationPlayer_Sword;

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

    public Coroutine AnimateSwordIntoCore()
    {
        return this.StartCoroutine(Cr);
        IEnumerator Cr()
        {
            yield return AnimationPlayer_Sword.PlayAndWaitForAnimation("sword_insert");
        }
    }
}

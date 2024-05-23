using Godot;
using System;

public partial class Touchable : StaticBody3D, IInteractable, ITouchable
{
    [Export]
    public string HoverText;

    public Node3D Node => this;
    public string InteractableText => HoverText;

    public event Action OnTouched;

    public override void _Ready()
    {
        base._Ready();
        NodeScript.FindNodesFromAttribute(this, GetType());
    }

    public void SetCollisionMode(InteractCollisionMode mode)
    {
        CollisionLayer = mode switch
        {
            InteractCollisionMode.All => CollisionMaskHelper.Create(1, 3),
            InteractCollisionMode.Collision => CollisionMaskHelper.Create(1),
            InteractCollisionMode.Interact => CollisionMaskHelper.Create(3),
            _ => 0
        };

        CollisionMask = mode switch
        {
            InteractCollisionMode.All => CollisionMaskHelper.Create(1),
            InteractCollisionMode.Collision => CollisionMaskHelper.Create(1),
            InteractCollisionMode.Interact => CollisionMaskHelper.Create(1),
            _ => 0
        };
    }

    public void Touch()
    {
        Debug.LogMethod();
        Debug.Indent++;

        OnTouched?.Invoke();

        Debug.Indent--;
    }
}

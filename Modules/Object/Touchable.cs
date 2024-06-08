using Godot;
using System;

public partial class Touchable : StaticBody3D, IInteractable, ITouchable
{
    [Export]
    public string HoverText;

    [Export]
    public InteractCollisionMode StartCollisionMode = InteractCollisionMode.All;

    public Node3D Node => this;
    public string InteractableText => HoverText;

    public event Action OnTouched;

    public override void _Ready()
    {
        base._Ready();
        NodeScript.FindNodesFromAttribute(this, GetType());
        SetCollisionMode(StartCollisionMode);
    }

    public void SetCollisionMode(InteractCollisionMode mode)
    {
        CollisionLayer = mode switch
        {
            InteractCollisionMode.All => CollisionMaskHelper.Create(CollisionMaskName.Player, CollisionMaskName.Item, CollisionMaskName.Interact),
            InteractCollisionMode.Collision => CollisionMaskHelper.Create(CollisionMaskName.Player, CollisionMaskName.Item),
            InteractCollisionMode.Interact => CollisionMaskHelper.Create(CollisionMaskName.Interact),
            _ => 0
        };

        CollisionMask = mode switch
        {
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

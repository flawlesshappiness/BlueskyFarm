using Godot;
using System.Collections.Generic;

public partial class Interactable : Node3DScript
{
    [Export]
    public string InteractableText;

    [Export]
    public bool OverrideInitialCollisionMode;

    public PhysicsBody3D Body => _body ??= this.GetNodeInChildren<PhysicsBody3D>();
    private PhysicsBody3D _body;

    public override void _Ready()
    {
        base._Ready();

        if (!OverrideInitialCollisionMode)
        {
            SetCollisionWithAll();
        }
    }

    public void SetCollisionWithAll()
    {
        Body.CollisionLayer = CollisionMaskHelper.Create(CollisionMaskType.Player, CollisionMaskType.Item, CollisionMaskType.Interact);
    }

    public void SetCollision(bool player = false, bool item = false, bool interact = false)
    {
        var list = new List<System.Enum>();

        if (player) list.Add(CollisionMaskType.Player);
        if (item) list.Add(CollisionMaskType.Item);
        if (interact) list.Add(CollisionMaskType.Interact);

        Body.CollisionLayer = CollisionMaskHelper.Create(list.ToArray());
    }

    public void SetCollisionNone() => SetCollision();
}

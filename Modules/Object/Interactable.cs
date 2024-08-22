using Godot;
using System.Collections.Generic;

public partial class Interactable : Node3DScript
{
    [Export]
    public string InteractableText;

    [Export]
    public bool OverrideInitialCollisionMode;

    [Export]
    public Texture2D OverrideCursorTexture;

    public PhysicsBody3D Body => _body ??= this.GetNodeInChildren<PhysicsBody3D>();
    private PhysicsBody3D _body;

    public override void _Ready()
    {
        base._Ready();

        if (!OverrideInitialCollisionMode)
        {
            SetCollision_Item();
        }
    }

    public void SetCollision_None()
    {
        SetCollisionLayer();
        SetCollisionMask();
    }

    public void SetCollision_All()
    {
        SetCollisionLayer(player: true, item: true, interact: true);
        SetCollisionMask(player: true, item: true, interact: true);
    }

    /// <summary>
    /// Is an interactable item, and collides with other items
    /// </summary>
    public void SetCollision_Item()
    {
        SetCollisionLayer(item: true, interact: true);
        SetCollisionMask(item: true);
    }

    /// <summary>
    /// Is an interactable, and collides with nothing
    /// </summary>
    public void SetCollision_Interactable()
    {
        SetCollisionLayer(interact: true);
        SetCollisionMask();
    }

    public void SetCollisionLayer(bool player = false, bool item = false, bool interact = false)
    {
        Body.CollisionLayer = CreateCollisionMask(player, item, interact);
    }

    public void SetCollisionMask(bool player = false, bool item = false, bool interact = false)
    {
        Body.CollisionMask = CreateCollisionMask(player, item, interact);
    }

    private uint CreateCollisionMask(bool player = false, bool item = false, bool interact = false)
    {
        var list = new List<System.Enum>();

        if (player) list.Add(CollisionMaskType.Player);
        if (item) list.Add(CollisionMaskType.Item);
        if (interact) list.Add(CollisionMaskType.Interact);

        return CollisionMaskHelper.Create(list.ToArray());
    }
}

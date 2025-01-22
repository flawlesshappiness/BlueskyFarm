using Godot;

public partial class InteractableStaticBody3D : StaticBody3D, IInteractable
{
    [Export]
    public string HoverText;

    [Export]
    public Texture2D HoverIcon;

    [Export]
    public Node3D HoverIconPosition;

    public PhysicsBody3D Body => this;

    public override void _Ready()
    {
        base._Ready();
        NodeScript.FindNodesFromAttribute(this, GetType());
    }

    public void SetCollisionLayer(params int[] masks)
    {
        CollisionLayer = CollisionMaskHelper.Create(masks);
    }

    public void SetCollisionMask(params int[] masks)
    {
        CollisionMask = CollisionMaskHelper.Create(masks);
    }

    public void ClearCollisionLayerAndMask()
    {
        SetCollisionLayer();
        SetCollisionMask();
    }

    public string GetHoverText() => HoverText;
    public Texture2D GetHoverIcon() => HoverIcon;
    public Vector3 GetHoverIconPosition() => IsInstanceValid(HoverIconPosition) ? HoverIconPosition.GlobalPosition : GlobalPosition;
}

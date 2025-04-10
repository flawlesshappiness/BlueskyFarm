using Godot;

public partial class InteractableRigidBody3D : RigidBody3D, IInteractable
{
    [Export]
    public string HoverText;

    [Export]
    public Texture2D HoverIcon;

    public bool Enabled { get; set; } = true;
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
    public Vector3 GetHoverIconPosition() => GlobalPosition;
}

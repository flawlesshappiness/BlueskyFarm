using Godot;
using System.Linq;

public partial class PlantArea : Area3D
{
    [NodeName("SeedPosition")]
    public Node3D SeedPosition;

    public override void _Ready()
    {
        base._Ready();
        NodeScript.FindNodesFromAttribute(this, GetType());

        BodyEntered += body => CallDeferred(nameof(OnBodyEntered), body);
    }

    private void OnBodyEntered(Node3D body)
    {
        var seed = body.GetNodeInChildren<Seed>();
        if (seed == null) return;
        if (seed.Planted) return;

        PlantSeed(body);
    }

    public void PlantSeed(Node3D node, Seed seed = null)
    {
        Debug.LogMethod($"{node}, {seed}");
        Debug.Indent++;

        if (node == null)
        {
            Debug.LogError("Node was null");
            Debug.Indent--;
            return;
        }

        seed ??= node.GetNodeInChildren<Seed>();
        if (seed == null)
        {
            Debug.LogError("Tried to find Seed on node, but found nothing");
            Debug.Indent--;
            return;
        }

        seed.Planted = true;

        ReparentSeed(node);

        Debug.Indent--;
    }

    private void ReparentSeed(Node3D node)
    {
        var grabbable = node as IGrabbable;
        if (grabbable != null)
        {
            grabbable.IsGrabbable = false;
        }

        grabbable.Node.GetNodesInChildren<CollisionShape3D>()
            .ToList()
            .ForEach(x => x.Disabled = true);

        var rig = grabbable as RigidBody3D;
        rig.Freeze = true;

        node.GlobalTransform = SeedPosition.GlobalTransform;
        node.SetParent(SeedPosition);
    }
}

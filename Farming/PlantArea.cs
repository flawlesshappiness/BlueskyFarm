using Godot;
using System.Collections;

public partial class PlantArea : Area3D
{
    [NodeName("SeedPosition")]
    public Node3D SeedPosition;

    public Seed CurrentSeed { get; private set; }

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

        CurrentSeed = seed;
        CurrentSeed.Planted = true;

        ReparentPlantedItem(node);
        BeginGrowing(CurrentSeed);

        Debug.Indent--;
    }

    private void ReparentPlantedItem(Node3D node)
    {
        var interactable = node as IInteractable;
        interactable.SetCollisionMode(InteractCollisionMode.None);

        var rig = interactable as RigidBody3D;
        rig.Freeze = true;

        node.GlobalTransform = SeedPosition.GlobalTransform;
        node.SetParent(SeedPosition);
    }

    private void BeginGrowing(Seed seed)
    {
        Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            yield return new WaitForSeconds(2f);

            FullyGrowSeed(seed);
        }
    }

    private void FullyGrowSeed(Seed seed)
    {
        // Create plant
        var plant = SpawnPlantFromSeed(seed);
        var grabbable = plant as IGrabbable;
        var interactable = plant as IInteractable;

        // Destroy seed
        var parent = seed.GetParent();
        parent.QueueFree();

        // Setup plant
        interactable.SetCollisionMode(InteractCollisionMode.Interact);
        grabbable.OnGrabbed += OnGrabbedPlant;

        void OnGrabbedPlant()
        {
            var rig = grabbable as RigidBody3D;
            rig.Freeze = false;

            interactable.SetCollisionMode(InteractCollisionMode.All);
            grabbable.OnGrabbed -= OnGrabbedPlant;

            grabbable.Node.SetParent(Scene.Root);
        }
    }

    private Node3D SpawnPlantFromSeed(Seed seed)
    {
        var plant = GDHelper.Instantiate<Node3D>(seed.PlantPath);
        ReparentPlantedItem(plant);

        return plant;
    }
}

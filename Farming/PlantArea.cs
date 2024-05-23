using Godot;
using System;

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
        SleepController.Instance.OnTicksChanged += OnSleep;
    }

    private void OnBodyEntered(Node3D body)
    {
        if (CurrentSeed != null) return;

        var item = body.GetNodeInChildren<Item>();
        if (item == null) return;
        if (!item.Info.CanPlant) return;
        if (item.PlantInfo == null) return;

        try
        {
            PlantSeed(item);
        }
        catch (Exception e)
        {
            Debug.LogError("Exception occured when body entered PlantArea");
            Debug.LogError(e.Message);
            Debug.LogError(e.StackTrace);
        }
    }

    public void PlantSeed(Item item)
    {
        Debug.LogMethod($"{item}");
        Debug.Indent++;

        if (item == null)
        {
            Debug.LogError("Item was null");
            Debug.Indent--;
            return;
        }

        if (item.PlantInfo == null)
        {
            Debug.LogError("item.PlantInfo was null");
            Debug.Indent--;
            return;
        }

        CurrentSeed = new Seed
        {
            PlantInfo = item.PlantInfo,
            GrowTicksStart = SleepController.Instance.CurrentTicks,
            GrowTicksEnd = SleepController.Instance.CurrentTicks + item.PlantInfo.GrowTicks,
            SeedModel = item
        };

        ReparentPlantedItem(item);

        Debug.Indent--;
    }

    private void ReparentPlantedItem(Node3D node)
    {
        var interactable = node as IInteractable;
        interactable.SetCollisionMode(InteractCollisionMode.None);

        var rig = node as RigidBody3D;
        rig.Freeze = true;

        node.GlobalTransform = SeedPosition.GlobalTransform;
        node.SetParent(SeedPosition);
    }

    private void DespawnSeedModel(Seed seed)
    {
        Debug.LogMethod();
        seed.SeedModel.QueueFree();
        seed.SeedModel = null;
    }

    public void SpawnPlantModel(Seed seed)
    {
        Debug.LogMethod();
        Debug.Indent++;

        if (seed == null)
        {
            Debug.LogError("Seed was null");
            Debug.Indent--;
            return;
        }

        seed.PlantModel = ItemController.Instance.CreateItem(seed.PlantInfo);
        ReparentPlantedItem(seed.PlantModel);
        SetupPlantGrabbable(seed);

        Debug.Indent--;
    }

    private void SetupPlantGrabbable(Seed seed)
    {
        if (seed == null) return;

        var grabbable = seed.GrabbablePlant;
        var interactable = seed.InteractablePlant;

        grabbable.OnGrabbed += OnGrabbedPlant;

        void OnGrabbedPlant()
        {
            var rig = grabbable as RigidBody3D;
            rig.Freeze = false;

            interactable.SetCollisionMode(InteractCollisionMode.All);
            grabbable.OnGrabbed -= OnGrabbedPlant;

            grabbable.Node.SetParent(Scene.Root);

            CurrentSeed = null;
        }
    }

    public void SetPlantGrowing(Seed seed)
    {
        if (seed == null) return;

        seed.InteractablePlant.SetCollisionMode(InteractCollisionMode.None);
        seed.PlantModel.Scale = Vector3.One * 0.5f;
    }

    public void SetPlantFullyGrown(Seed seed)
    {
        if (seed == null) return;

        seed.PlantModel.Scale = Vector3.One * 1f;
        seed.InteractablePlant.SetCollisionMode(InteractCollisionMode.Interact);
    }

    private void OnSleep()
    {
        if (CurrentSeed == null) return;

        Debug.LogMethod();
        Debug.Indent++;

        if (CurrentSeed.SeedModel != null)
        {
            DespawnSeedModel(CurrentSeed);
        }

        if (CurrentSeed.PlantModel == null)
        {
            SpawnPlantModel(CurrentSeed);
        }

        if (SleepController.Instance.CurrentTicks >= CurrentSeed.GrowTicksEnd)
        {
            SetPlantFullyGrown(CurrentSeed);
        }
        else
        {
            SetPlantGrowing(CurrentSeed);
        }

        Debug.Indent--;
    }
}

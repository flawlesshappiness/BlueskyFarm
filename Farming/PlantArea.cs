using Godot;
using System;
using System.Linq;

public partial class PlantArea : Area3D
{
    [NodeName("SeedPosition")]
    public Node3D SeedPosition;

    public Seed CurrentSeed { get; private set; }

    private bool _initialized;
    private Item _parent_item;

    public override void _Ready()
    {
        base._Ready();
        NodeScript.FindNodesFromAttribute(this, GetType());

        BodyEntered += body => CallDeferred(nameof(OnBodyEntered), body);

        _parent_item = this.GetNodeInParents<Item>();
        _parent_item.OnUpdateData += UpdateData;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (!_initialized)
        {
            _initialized = true;
            Initialize();
        }

        Process_Grow();
    }

    private void Initialize()
    {
        LoadData();
    }

    private void OnBodyEntered(Node3D body)
    {
        if (CurrentSeed != null) return;

        var item = body.GetNodeInChildren<Item>();
        if (item == null) return;
        if (!item.Info.CanPlant) return;
        if (string.IsNullOrEmpty(item.Data.PlantInfoPath)) return;

        var plant_info = GD.Load<ItemInfo>(item.Data.PlantInfoPath);
        if (plant_info == null) return;

        ItemController.Instance.UntrackItem(item);
        item.QueueFree();

        var data = new PlantAreaData
        {
            ItemId = _parent_item.Data.Id,
            ItemInfoPath = item.Data.PlantInfoPath,
            TimeLeft = plant_info.GrowTimeInSeconds
        };

        AddOrSetData(data);
        PlantSeedFromData(data);
    }

    private PlantAreaData GetOrCreateData()
    {
        var id = _parent_item.Data.Id;
        var scene_data = Scene.Current.SceneData;
        var data = scene_data.PlantAreas.FirstOrDefault(x => x.ItemId == id);

        if (data == null)
        {
            data = new PlantAreaData { ItemId = id };
            scene_data.PlantAreas.Add(data);
        }

        return data;
    }

    private void RemoveData(Guid id)
    {
        var scene_data = Scene.Current.SceneData;
        var other_data = scene_data.PlantAreas.FirstOrDefault(x => x.ItemId == id);
        if (other_data != null)
        {
            scene_data.PlantAreas.Remove(other_data);
        }
    }

    private void AddOrSetData(PlantAreaData data)
    {
        RemoveData(data.ItemId);
        Scene.Current.SceneData.PlantAreas.Add(data);
    }

    private void LoadData()
    {
        var data = GetOrCreateData();

        if (string.IsNullOrEmpty(data.ItemInfoPath)) return;

        PlantSeedFromData(data);
    }

    private void PlantSeedFromData(PlantAreaData data)
    {
        Debug.LogMethod($"{data.ItemInfoPath}");
        Debug.Indent++;

        if (data == null)
        {
            Debug.LogError("data was null");
            Debug.Indent--;
            return;
        }

        var seed_item_path = ItemController.Instance.Collection.Seed;
        var seed_item = ItemController.Instance.CreateItem(seed_item_path);
        ReparentItem(seed_item);

        CurrentSeed = new Seed
        {
            Data = data,
            TimeStart = GameTime.Time,
            TimeEnd = GameTime.Time + data.TimeLeft,
            SeedModel = seed_item
        };

        Debug.Indent--;
    }

    private void ReparentItem(Node3D node)
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

        if (seed == null) return;
        if (seed.SeedModel == null) return;

        ItemController.Instance.UntrackItem(seed.SeedModel as Item);
        seed.SeedModel.QueueFree();
        seed.SeedModel = null;
    }

    private void SpawnPlantFromData(PlantAreaData data)
    {
        Debug.LogMethod($"{data.ItemInfoPath}");
        Debug.Indent++;

        if (data == null)
        {
            Debug.LogError("data was null");
            Debug.Indent--;
            return;
        }

        CurrentSeed = new Seed
        {
            Data = data,
            TimeStart = GameTime.Time,
            TimeEnd = GameTime.Time,
        };

        SpawnPlantModel(CurrentSeed);

        Debug.Indent--;
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

        var plant = ItemController.Instance.CreateItem(seed.Data.ItemInfoPath);
        ItemController.Instance.UntrackItem(plant);

        seed.PlantModel = plant;

        ReparentItem(seed.PlantModel);
        SetupPlantGrabbable(seed);
        SetPlantFullyGrown(CurrentSeed);

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

            RemoveData(CurrentSeed.Data.ItemId);

            CurrentSeed = null;
        }
    }

    public void SetPlantFullyGrown(Seed seed)
    {
        if (seed == null) return;

        seed.PlantModel.Scale = Vector3.One * 1f;
        seed.InteractablePlant.SetCollisionMode(InteractCollisionMode.Interact);
    }

    private void Process_Grow()
    {
        if (CurrentSeed == null) return; // No seed
        if (CurrentSeed.PlantModel != null) return; // Already has plant

        if (GameTime.Time > CurrentSeed.TimeEnd)
        {
            DespawnSeedModel(CurrentSeed);
            SpawnPlantModel(CurrentSeed);
        }
    }

    private void UpdateData()
    {
        if (CurrentSeed == null) return;
        CurrentSeed.UpdateData();
    }
}

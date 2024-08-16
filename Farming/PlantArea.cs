using Godot;
using System;
using System.Collections;
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

        var item = body.GetNodeInParents<Item>();
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
        Debug.TraceMethod($"{data.ItemInfoPath}");
        Debug.Indent++;

        if (data == null)
        {
            Debug.LogError("data was null");
            Debug.Indent--;
            return;
        }

        var seed_item_path = ItemController.Instance.Collection.Seed;
        var seed_item = ItemController.Instance.CreateItem(seed_item_path, track_item: false);
        ReparentItem(seed_item);

        CurrentSeed = new Seed
        {
            Data = data,
            TimeStart = GameTime.Time,
            TimeEnd = GameTime.Time + data.TimeLeft,
            SeedItem = seed_item
        };

        Debug.Indent--;
    }

    private void ReparentItem(Item item)
    {
        item.SetCollision_None();
        item.RigidBody.Freeze = true;
        item.GlobalTransform = SeedPosition.GlobalTransform;
        item.SetParent(SeedPosition);
        item.ResetBodyPosition();
        item.ResetBodyRotation();
    }

    private void DespawnSeedModel(Seed seed)
    {
        Debug.TraceMethod();

        if (seed == null) return;
        if (seed.SeedItem == null) return;

        ItemController.Instance.UntrackItem(seed.SeedItem);
        seed.SeedItem.QueueFree();
        seed.SeedItem = null;
    }

    private void SpawnPlantFromData(PlantAreaData data)
    {
        Debug.TraceMethod($"{data.ItemInfoPath}");
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
        Debug.TraceMethod();
        Debug.Indent++;

        if (seed == null)
        {
            Debug.LogError("Seed was null");
            Debug.Indent--;
            return;
        }

        seed.PlantItem = ItemController.Instance.CreateItem(seed.Data.ItemInfoPath, track_item: false);
        ReparentItem(seed.PlantItem);
        SetupPlantGrabbable(seed);
        SetPlantFullyGrown(CurrentSeed);

        Debug.Indent--;
    }

    private void SetupPlantGrabbable(Seed seed)
    {
        if (seed == null) return;

        var item = seed.PlantItem;

        item.OnGrabbed += OnGrabbedPlant;
        item.OnAddedToInventory += OnGrabbedPlant;

        void OnGrabbedPlant()
        {
            item.OnGrabbed -= OnGrabbedPlant;
            item.OnAddedToInventory -= OnGrabbedPlant;
            item.RigidBody.Freeze = false;
            item.SetCollision_Item();
            item.SetParent(Scene.Root);

            RemoveData(CurrentSeed.Data.ItemId);

            ItemController.Instance.TrackItem(item);

            CurrentSeed = null;
        }
    }

    public void SetPlantFullyGrown(Seed seed)
    {
        if (seed == null) return;

        seed.PlantItem.Scale = Vector3.One * 1f;
        seed.PlantItem.SetCollision_Interactable();
    }

    private void Process_Grow()
    {
        if (CurrentSeed == null) return; // No seed
        if (CurrentSeed.PlantItem != null) return; // Already has plant

        if (GameTime.Time > CurrentSeed.TimeEnd)
        {
            DespawnSeedModel(CurrentSeed);
            SpawnPlantModel(CurrentSeed);
            AnimatePlantAppear(CurrentSeed);

            SoundController.Instance.Play("sfx_pickup", new SoundSettings3D
            {
                Bus = SoundBus.SFX,
                Position = GlobalPosition
            });
        }
    }

    private void UpdateData()
    {
        if (CurrentSeed == null) return;
        CurrentSeed.UpdateData();
    }

    private void AnimatePlantAppear(Seed seed)
    {
        Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            var duration = 0.05f;
            var plant = seed.PlantItem;
            var scale_A = Vector3.One * 1.1f;
            var scale_B = Vector3.One * 1.2f;
            var scale_C = Vector3.One * 0.9f;
            var scale_D = Vector3.One * 1.0f;

            yield return LerpEnumerator.Lerp01(duration, f =>
            {
                plant.Scale = Lerp.Vector(scale_A, scale_B, Curves.EaseOutQuad.Evaluate(f));
            });

            yield return LerpEnumerator.Lerp01(duration * 0.9f, f =>
            {
                plant.Scale = Lerp.Vector(scale_B, scale_C, Curves.EaseInOutQuad.Evaluate(f));
            });

            yield return LerpEnumerator.Lerp01(duration * 0.8f, f =>
            {
                plant.Scale = Lerp.Vector(scale_C, scale_D, Curves.EaseInOutQuad.Evaluate(f));
            });
        }
    }
}

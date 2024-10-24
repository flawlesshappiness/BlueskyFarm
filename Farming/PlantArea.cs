using Godot;
using Godot.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[Tool]
public partial class PlantArea : Touchable
{
    [Export]
    public string Id;

    [Export]
    public Vector3 WeedArea;

    [NodeType]
    public Area3D Area;

    [NodeName]
    public Node3D SeedPosition;

    [NodeType]
    public Weed WeedTemplate;

    [NodeName]
    public Path3D SeedPath;

    [NodeName]
    public GpuParticles3D ps_dirt_puff;

    public Seed CurrentSeed { get; private set; }

    private const float WEED_TIME = 10f;

    private List<Weed> _current_weeds = new();

    protected override void _ReadyPlayer()
    {
        base._ReadyPlayer();

        Area.BodyEntered += body => CallDeferred(nameof(OnBodyEntered), body);
        WeedTemplate.SetEnabled(false);
    }

    protected override void _ProcessPlayer(double delta)
    {
        base._ProcessPlayer(delta);
        Process_Grow();
        Process_Weeds();
    }

    public override void _ValidateProperty(Dictionary property)
    {
        base._ValidateProperty(property);

        if (property["name"].AsStringName() == nameof(Id))
        {
            var flags = property["usage"].As<PropertyUsageFlags>();
            flags |= PropertyUsageFlags.ReadOnly;
            property["usage"] = (int)flags;
        }
    }

    protected override void Initialize()
    {
        base.Initialize();
        LoadData();
    }

    private void LoadData()
    {
        var data = GetOrCreateData();

        if (string.IsNullOrEmpty(data.PlantInfoPath)) return;

        if (data.TimeLeft <= 0)
        {
            SpawnPlantFromData(data);
        }
        else
        {
            PlantSeedFromData(data);
        }

        CreateWeedsFromData(data);
    }

    public void UpdateData()
    {
        if (CurrentSeed == null) return;
        CurrentSeed.UpdateData();
    }

    private void OnBodyEntered(Node3D body)
    {
        if (CurrentSeed != null) return;

        var item = body.GetNodeInParents<Item>();
        if (item == null) return;
        if (!item.Info.IsSeed) return;
        if (string.IsNullOrEmpty(item.Data.PlantInfoPath)) return;

        var plant_info = GD.Load<ItemInfo>(item.Data.PlantInfoPath);
        if (plant_info == null) return;

        ItemController.Instance.UntrackItem(item);
        var item_position = item.GlobalPosition;
        item.QueueFree();

        var data = new PlantAreaData
        {
            Id = Id,
            PlantInfoPath = item.Data.PlantInfoPath,
            TimeLeft = plant_info.GrowTimeInSeconds,
        };

        SetData(data);
        PlantSeedFromData(data);

        CurrentSeed.SeedItem.GlobalPosition = item_position;
        PlayThrowSFX();
        AnimateSeedToGround(CurrentSeed.SeedItem);
    }

    private PlantAreaData GetOrCreateData()
    {
        var scene_data = Scene.Current.SceneData;
        var data = scene_data.PlantAreas.FirstOrDefault(x => x.Id == Id);

        if (data == null)
        {
            data = new PlantAreaData { Id = Id };
            scene_data.PlantAreas.Add(data);
        }

        return data;
    }

    private void RemoveSeedData(string id)
    {
        var scene_data = Scene.Current.SceneData;
        var other_data = scene_data.PlantAreas.FirstOrDefault(x => x.Id == id);
        if (other_data != null)
        {
            scene_data.PlantAreas.Remove(other_data);
        }
    }

    private void SetData(PlantAreaData data)
    {
        RemoveSeedData(data.Id);
        Scene.Current.SceneData.PlantAreas.Add(data);
    }

    private void PlantSeedFromData(PlantAreaData data)
    {
        Debug.TraceMethod($"{data.PlantInfoPath}");
        Debug.Indent++;

        if (data == null)
        {
            Debug.LogError("data was null");
            Debug.Indent--;
            return;
        }

        var seed_item_path = ItemController.Instance.Collection.Seed;
        var seed_item = ItemController.Instance.CreateItem(seed_item_path, track_item: false);
        SetItemPosition(seed_item);

        CurrentSeed = new Seed
        {
            Data = data,
            TimeStart = GameTime.Time,
            TimeEnd = GameTime.Time + data.TimeLeft,
            TimeWeed = GameTime.Time + WEED_TIME,
            SeedItem = seed_item
        };

        Debug.Indent--;
    }

    private void SetItemPosition(Item item)
    {
        item.GlobalPosition = SeedPosition.GlobalPosition;
        item.GlobalRotation = SeedPosition.GlobalRotation;
        item.LockPosition_All();
        item.LockRotation_All();
        item.SetCollision_None();
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
        Debug.TraceMethod($"{data.PlantInfoPath}");
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

        seed.PlantItem = ItemController.Instance.CreateItem(seed.Data.PlantInfoPath, track_item: false);

        SetItemPosition(seed.PlantItem);
        RandomizeScale(seed.PlantItem);

        Debug.Indent--;
    }

    private void RandomizeScale(Item item)
    {
        var rng = new RandomNumberGenerator();
        var is_huge = rng.Randf() < 0.05f;

        if (is_huge)
        {
            item.Data.Scale = 2.5f;
            item.Data.PlantIsHuge = true;
        }
        else
        {
            var scale = rng.RandfRange(0.75f, 1.0f);
            item.Data.Scale = scale;
        }
    }

    private void Process_Grow()
    {
        if (CurrentSeed == null) return; // No seed
        if (CurrentSeed.PlantItem != null) return; // Already has plant

        if (IsSeedFinishedGrowing())
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

    private bool HasSeed()
    {
        return CurrentSeed != null;
    }

    private bool IsSeedFinishedGrowing()
    {
        return GameTime.Time > CurrentSeed.TimeEnd;
    }

    public override bool HandleCursor()
    {
        if (CurrentSeed == null)
        {
            Cursor.Hide();
            return true;
        }
        else if (IsSeedFinishedGrowing())
        {
            return false;
        }
        else
        {
            var seconds = (int)(CurrentSeed.TimeEnd - GameTime.Time);
            var end_date = new TimeSpan(0, 0, seconds);
            Cursor.Show(new CursorSettings
            {
                Name = "Wait",
                Position = SeedPosition.GlobalPosition,
                Text = end_date.ToString("mm':'ss")
            });
            return true;
        }
    }

    protected override void Touched()
    {
        base.Touched();
        if (CurrentSeed?.PlantItem == null) return;
        if (HasWeeds()) return;

        var data = CurrentSeed.PlantItem.Data;
        PopPlantFromDirt(data);

        var rng = new RandomNumberGenerator();
        var is_double = rng.Randf() < 0.05f;
        if (is_double)
        {
            PopPlantFromDirt(data);
        }

        PlayDirtPuffParticle();
        PlayDirtSFX();

        CurrentSeed.PlantItem.QueueFree();
        RemoveSeedData(CurrentSeed.Data.Id);
        CurrentSeed = null;
    }

    private void PopPlantFromDirt(ItemData data)
    {
        var rng = new RandomNumberGenerator();
        var dir_to_player = (FirstPersonController.Instance.Mid.GlobalPosition - SeedPosition.GlobalPosition).Normalized();
        var velocity = Vector3.Up * 2f + dir_to_player * 3f;
        var torque = new Vector3().RandomNormalized() * rng.RandfRange(1f, 5f);
        var item = ItemController.Instance.CreateItem(data.InfoPath);

        item.SetScale(data.Scale);
        item.GlobalPosition = SeedPosition.GlobalPosition.Add(y: 0.25f);
        item.RigidBody.LinearVelocity = velocity;
        item.RigidBody.AngularVelocity = torque;
    }

    private void AnimatePlantAppear(Seed seed)
    {
        PlayDirtPuffParticle();
        seed.PlantItem.AnimateWobble();
    }

    private void AnimateSeedToGround(Item item)
    {
        var p1 = item.GlobalPosition;
        var p1_out = Vector3.Up * 0.5f;
        var p2 = SeedPosition.GlobalPosition;
        var p2_in = Vector3.Up * 1.5f;
        var origin = SeedPath.GlobalPosition;

        var curve = new Curve3D();
        curve.ClearPoints();
        curve.AddPoint(p1 - origin, @out: p1_out);
        curve.AddPoint(p2 - origin, @in: p2_in);
        SeedPath.Curve = curve;

        Coroutine.Start(Cr, this);
        IEnumerator Cr()
        {
            var length = curve.GetBakedLength();
            var duration = 0.5f;
            var time_start = GameTime.Time;
            var time_end = time_start + duration;

            while (GameTime.Time < time_end - 0.1f)
            {
                var t = GameTime.T(time_start, duration);
                var offset = Mathf.Lerp(0f, length, t);
                var position = curve.SampleBaked(offset);
                item.GlobalPosition = origin + position;
                yield return null;
            }

            item.GlobalPosition = SeedPosition.GlobalPosition;

            PlayDirtPuffParticle();
            PlayDirtSFX();
        }
    }

    private void PlayDirtPuffParticle()
    {
        ps_dirt_puff.Emitting = true;
    }

    private void PlayDirtSFX()
    {
        SoundController.Instance.Play("sfx_seed_in_dirt", new SoundSettings3D
        {
            Bus = SoundBus.SFX,
            Position = SeedPosition.GlobalPosition,
            PitchMin = 0.95f,
            PitchMax = 1f
        });
    }

    private void PlayThrowSFX()
    {
        SoundController.Instance.Play("sfx_throw_light", new SoundSettings3D
        {
            Bus = SoundBus.SFX,
            Position = SeedPosition.GlobalPosition,
            PitchMin = 0.95f,
            PitchMax = 1f
        });
    }

    private void ClearWeeds()
    {
        foreach (var weed in _current_weeds.ToList())
        {
            weed.QueueFree();
        }

        _current_weeds.Clear();
        CurrentSeed.Data.WeedCount = 0;
    }

    private void CreateWeedsFromData(PlantAreaData data)
    {
        if (data == null) return;

        var count_to_spawn = data.WeedCount;
        while (data.TimeUntilNextWeed < 0)
        {
            data.TimeUntilNextWeed += WEED_TIME;
            count_to_spawn++;
        }

        for (int i = 0; i < count_to_spawn; i++)
        {
            CreateWeed();
        }

        CurrentSeed.TimeWeed = GameTime.Time + data.TimeUntilNextWeed;
    }

    private Weed CreateWeed()
    {
        var weed = WeedTemplate.Duplicate() as Weed;
        weed.SetParent(WeedTemplate.GetParent());
        weed.SetEnabled(weed.IsTouchable);
        weed.RandomizeModel();
        weed.RandomizeScale();
        weed.AnimateAppear();
        weed.OnWeedCut += () => WeedCut(weed);

        var rng = new RandomNumberGenerator();
        var w = WeedArea.X * 0.5f;
        var h = WeedArea.Z * 0.5f;
        var x = rng.RandfRange(-w, w);
        var z = rng.RandfRange(-h, h);
        weed.GlobalPosition = SeedPosition.GlobalPosition + new Vector3(x, 0, z);

        _current_weeds.Add(weed);
        CurrentSeed.Data.WeedCount++;
        return weed;
    }

    private void WeedCut(Weed weed)
    {
        _current_weeds.Remove(weed);
        CurrentSeed.Data.WeedCount--;
        CurrentSeed.DecreaseTimeFromWeeds();

        CurrentSeed?.SeedItem?.AnimateWobble();
        CurrentSeed?.PlantItem?.AnimateWobble();
    }

    private bool HasWeeds()
    {
        return _current_weeds.Count > 0;
    }

    private void Process_Weeds()
    {
        if (!HasSeed()) return;
        if (IsSeedFinishedGrowing()) return;

        if (GameTime.Time > CurrentSeed.TimeWeed)
        {
            var rng = new RandomNumberGenerator();
            CurrentSeed.TimeWeed += WEED_TIME * rng.RandfRange(0.9f, 1.1f);
            CreateWeed();
        }
    }
}

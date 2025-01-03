using Godot;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class PlantArea : Node3DScript
{
    [Export]
    public string Id;

    [NodeName]
    public Node3D Model;

    [Export]
    public Vector3 WeedArea;

    [NodeName]
    public Touchable TouchPlant;

    [NodeType]
    public Area3D Area;

    [NodeName]
    public Node3D SeedPosition;

    [NodeType]
    public Weed WeedTemplate;

    [NodeType]
    public Waterable Waterable;

    [NodeName]
    public Sprite3D WaterSprite;

    public Seed CurrentSeed { get; private set; }
    public bool HasSeed => CurrentSeed != null;

    private const float WEED_TIME = 20f;

    private List<Weed> _current_weeds = new();

    protected override void _ReadyPlayer()
    {
        base._ReadyPlayer();

        Area.BodyEntered += body => CallDeferred(nameof(OnBodyEntered), body);
        WeedTemplate.SetEnabled(false);

        Waterable.OnWatered += Watered;
        TouchPlant.OnTouched += Touched;
    }

    protected override void _ProcessPlayer(double delta)
    {
        base._ProcessPlayer(delta);
        Process_Grow();
        Process_Weeds();
    }

    protected override void Initialize()
    {
        base.Initialize();
        LoadData();
        UpdateTouchable();
    }

    private void LoadData()
    {
        var data = GetOrCreateData();

        SetWatered(data.IsWatered);

        if (string.IsNullOrEmpty(data.PlantInfo)) return;

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
        if (item.Info.Type != ItemType.Seed) return;
        if (string.IsNullOrEmpty(item.Data.Seed?.Info)) return;

        var plant_info = GD.Load<ItemInfo>(item.Data.Seed.Info);
        if (plant_info == null) return;

        ItemController.Instance.UntrackItem(item);
        var item_position = item.GlobalPosition;
        item.QueueFree();

        var data = new PlantAreaData
        {
            Id = Id,
            PlantInfo = item.Data.Seed.Info,
            TimeLeft = item.Data.Seed.OverrideGrowTime ?? plant_info.GrowTimeInSeconds,
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
        Debug.TraceMethod($"{data.PlantInfo}");
        Debug.Indent++;

        if (data == null)
        {
            Debug.LogError("data was null");
            Debug.Indent--;
            return;
        }

        var seed_item = ItemController.Instance.CreateItem("Seed_Plant", new CreateItemSettings
        {
            Tracked = false
        });

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
        item.ClearCollisionLayerAndMask();
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
        Debug.TraceMethod($"{data.PlantInfo}");
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
            TimeEnd = GameTime.Time + data.TimeLeft,
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

        seed.PlantItem = ItemController.Instance.CreateItemFromPath(seed.Data.PlantInfo, new CreateItemSettings
        {
            Tracked = false
        });

        SetItemPosition(seed.PlantItem);
        RandomizeScale(seed.PlantItem);

        Debug.Indent--;
    }

    private void RandomizeScale(Item item)
    {
        var rng = new RandomNumberGenerator();
        var is_huge = rng.Randf() < 0.05f;

        if (is_huge && false)
        {
            item.Data.Scale = 2.5f;
            //item.Data.Seed.IsHuge = true;
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
        if (!CurrentSeed.IsFinished) return;

        DespawnSeedModel(CurrentSeed);
        SpawnPlantModel(CurrentSeed);
        AnimatePlantAppear(CurrentSeed);

        SoundController.Instance.Play("sfx_pickup", GlobalPosition);
    }

    private void UpdateTouchable()
    {
        TouchPlant.SetEnabled(CurrentSeed != null && CurrentSeed.Data.WeedCount <= 0);
    }

    private void Touched()
    {
        if (CurrentSeed?.PlantItem == null) return;
        if (HasWeeds()) return;

        var data = CurrentSeed.PlantItem.Data;
        PopPlantFromDirt(data);
        AnimateUnwater();

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
        UpdateTouchable();
    }

    private void PopPlantFromDirt(ItemData data)
    {
        var rng = new RandomNumberGenerator();
        var dir_to_player = (Player.Instance.GlobalPosition.Add(y: 1) - SeedPosition.GlobalPosition).Normalized();
        var velocity = Vector3.Up * 2f + dir_to_player * 3f;
        var torque = new Vector3().RandomNormalized() * rng.RandfRange(1f, 5f);
        var item = ItemController.Instance.CreateItemFromPath(data.Info);

        item.SetScale(data.Scale);
        item.GlobalPosition = SeedPosition.GlobalPosition.Add(y: 0.25f);
        item.LinearVelocity = velocity;
        item.AngularVelocity = torque;

        SoundController.Instance.Play("sfx_seed_complete", GlobalPosition);
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

        var curve = new Curve3D();
        curve.ClearPoints();
        curve.AddPoint(p1, @out: p1_out);
        curve.AddPoint(p2, @in: p2_in);

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
                item.GlobalPosition = position;
                yield return null;
            }

            item.GlobalPosition = SeedPosition.GlobalPosition;

            UpdateTouchable();
            PlayDirtPuffParticle();
            PlayDirtSFX();
        }
    }

    private void PlayDirtPuffParticle()
    {
        Particle.PlayOneShot("ps_dirt_puff", SeedPosition.GlobalPosition);
    }

    private void PlayDirtSFX()
    {
        SoundController.Instance.Play("sfx_seed_in_dirt", SeedPosition.GlobalPosition);
    }

    private void PlayThrowSFX()
    {
        SoundController.Instance.Play("sfx_throw_light", SeedPosition.GlobalPosition);
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
        while (data.TimeUntilNextWeed < Mathf.Min(0, data.TimeLeft)) // Less than 0, or however low TimeLeft is (could be negative)
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

    private PlantWeed CreateWeed()
    {
        var weed = WeedTemplate.Duplicate() as PlantWeed;
        weed.SetParent(WeedTemplate.GetParent());
        weed.Enable();
        weed.RandomizeModel();
        weed.RandomizeScale();
        weed.OnWeedCut += () => WeedCut(weed);

        var rng = new RandomNumberGenerator();
        var w = WeedArea.X * 0.5f;
        var h = WeedArea.Z * 0.5f;
        var x = rng.RandfRange(-w, w);
        var z = rng.RandfRange(-h, h);
        weed.GlobalPosition = SeedPosition.GlobalPosition + new Vector3(x, 0, z);

        _current_weeds.Add(weed);
        return weed;
    }

    private Weed CreateWeedWithData()
    {
        var weed = CreateWeed();
        weed.AnimateAppear();
        CurrentSeed.Data.WeedCount++;
        UpdateTouchable();
        return weed;
    }

    private void WeedCut(Weed weed)
    {
        _current_weeds.Remove(weed);
        CurrentSeed.Data.WeedCount--;
        CurrentSeed.DecreaseTimeFromWeeds();

        CurrentSeed?.SeedItem?.AnimateWobble();
        CurrentSeed?.PlantItem?.AnimateWobble();

        UpdateTouchable();

        SoundController.Instance.Play("sfx_seed_grow", GlobalPosition);
    }

    private bool HasWeeds()
    {
        return _current_weeds.Count > 0;
    }

    private void Process_Weeds()
    {
        if (!HasSeed) return;
        if (CurrentSeed.IsFinished) return;

        if (GameTime.Time > CurrentSeed.TimeWeed)
        {
            var rng = new RandomNumberGenerator();
            CurrentSeed.TimeWeed += WEED_TIME * rng.RandfRange(0.9f, 1.1f);
            CreateWeedWithData();
        }
    }

    private void Watered()
    {
        if (!HasSeed) return;
        if (CurrentSeed.Data.IsWatered) return;

        CurrentSeed.Data.IsWatered = true;

        var grow_time = CurrentSeed.TimeEnd - GameTime.Time;
        grow_time *= 0.5f;
        CurrentSeed.TimeEnd = GameTime.Time + grow_time;

        AnimateWater();
        CurrentSeed?.SeedItem?.AnimateWobble();
        CurrentSeed?.PlantItem?.AnimateWobble();

        SoundController.Instance.Play("sfx_seed_grow", GlobalPosition);
    }

    private void AnimateWater()
    {
        var rng = new RandomNumberGenerator();
        WaterSprite.RotationDegrees = WaterSprite.RotationDegrees.Set(y: rng.RandfRange(0, 360f));

        this.StartCoroutine(Cr, "water");
        IEnumerator Cr()
        {
            var duration = 0.5f;
            var scale_start = 0.2f;
            var scale_end = 0.3f;
            var alpha_start = 0f;
            var alpha_end = 0.8f;
            var color = WaterSprite.Modulate;
            yield return LerpEnumerator.Lerp01(duration, f =>
            {
                WaterSprite.Scale = Vector3.One * Mathf.Lerp(scale_start, scale_end, f);
                WaterSprite.Modulate = color.SetA(Mathf.Lerp(alpha_start, alpha_end, f));
            });
        }
    }

    private void AnimateUnwater()
    {
        this.StartCoroutine(Cr, "water");
        IEnumerator Cr()
        {
            var duration = 0.25f;
            var alpha_start = WaterSprite.Modulate.A;
            var alpha_end = 0f;
            var color = WaterSprite.Modulate;
            yield return LerpEnumerator.Lerp01(duration, f =>
            {
                WaterSprite.Modulate = color.SetA(Mathf.Lerp(alpha_start, alpha_end, f));
            });
        }
    }

    private void SetWatered(bool watered)
    {
        var a = watered ? 0.8f : 0f;
        WaterSprite.Modulate = WaterSprite.Modulate.SetA(a);
    }

    public Coroutine AnimateAppear()
    {
        Model.Scale = Vector3.One * 0.01f;
        Model.Show();

        return this.StartCoroutine(Cr, "animate");
        IEnumerator Cr()
        {
            var curve = Curves.EaseOutBack;
            var start = Model.Scale;
            var end = Vector3.One;
            yield return LerpEnumerator.Lerp01(0.25f, f =>
            {
                var t = curve.Evaluate(f);
                Model.Scale = start.Lerp(end, t);
            });
        }
    }
}

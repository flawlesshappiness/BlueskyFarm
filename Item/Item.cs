using Godot;
using System;
using System.Collections;

public partial class Item : Grabbable
{
    [Export]
    public bool OverrideCollisionMode;

    public ItemInfo Info { get; set; }
    public ItemData Data { get; set; }
    public bool IsBeingHandled { get; set; } // If the item is currently being handled by a script, other scripts will ignore this item

    public event Action OnUpdateData;

    private bool _initialized;

    public override void _Ready()
    {
        base._Ready();
        BodyEntered += OnBodyEntered;

        ContactMonitor = true;
        MaxContactsReported = 1;

        if (!OverrideCollisionMode)
        {
            SetCollisionLayer(2, 3);
            SetCollisionMask(2);
        }
    }

    protected virtual void Initialize()
    {
        if (Info != null)
        {
            HoverText = string.IsNullOrEmpty(Info.ItemName) ? HoverText : Info.ItemName;
        }
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (!_initialized)
        {
            _initialized = true;
            Initialize();
        }

        if (GlobalPosition.Y < -50)
        {
            var bounds = FarmBounds.Instance;
            if (bounds != null)
            {
                bounds.ThrowObject(this, Player.Instance.GlobalPosition);
            }
        }
    }

    public void UpdateData()
    {
        var p = GlobalPosition;
        Data.X_Position = p.X;
        Data.Y_Position = p.Y;
        Data.Z_Position = p.Z;

        var r = GlobalRotation;
        Data.X_Rotation = r.X;
        Data.Y_Rotation = r.Y;
        Data.Z_Rotation = r.Z;

        OnUpdateData?.Invoke();
    }

    public virtual void LoadFromData()
    {
        GlobalPosition = new Vector3(Data.X_Position, Data.Y_Position, Data.Z_Position);
        GlobalRotation = new Vector3(Data.X_Rotation, Data.Y_Rotation, Data.Z_Rotation);
        Scale = Vector3.One * Data.Scale;
    }

    private void OnBodyEntered(Node other)
    {
        var vel_min = 1f;
        var vel_max = 5f;
        var vol_min = 0f;
        var vol_max = 30f;
        var pitch_min = 0.5f;
        var pitch_max = 1.5f;

        var rig_other = other as RigidBody3D;
        var vel_other = -rig_other?.LinearVelocity ?? Vector3.Zero;
        var avg_mul = rig_other == null ? 1 : 0.5f;
        var vel = LinearVelocity;
        var vel_avg = (vel + vel_other) * avg_mul;

        if (vel_avg.Length() < vel_min) return;

        var t_vel = Mathf.Clamp((vel_avg.Length() - vel_min) / (vel_max - vel_min), 0, 1);
        var volume = Mathf.Lerp(vol_min, vol_max, t_vel);
        var pitch = Mathf.Lerp(pitch_max, pitch_min, t_vel);

        var asp = SoundController.Instance.Play("sfx_impact_default", GlobalPosition);
        asp.VolumeDb = volume;
        asp.PitchScale = pitch;
    }

    public virtual void Use()
    {
        // Use item
    }

    public void ReplenishUses(int uses)
    {
        Data.Uses = Mathf.Clamp(Data.Uses + uses, 0, Info.Uses);
    }

    public virtual void PickUp()
    {
        InventoryController.Instance.PickUpItem(this);
    }

    public void SetScale(float scale)
    {
        Data.Scale = scale;
        Scale = Vector3.One * scale;
    }

    public Coroutine AnimateWobble()
    {
        return this.StartCoroutine(Cr, "animate");
        IEnumerator Cr()
        {
            var duration = 0.6f;
            var curve = AnimationCurves.WobbleOut(0.4f, 3);
            yield return LerpEnumerator.Lerp01(duration, f =>
            {
                var t = curve.Sample(f);
                Scale = Vector3.One * (Data.Scale * t);
            });
        }
    }

    public Coroutine AnimateDisappearAndQueueFree()
    {
        this.LockPosition_All();
        this.LockRotation_All();
        ClearCollisionLayerAndMask();
        IsBeingHandled = true;
        ItemController.Instance.UntrackItem(this);

        return this.StartCoroutine(Cr, "animate");
        IEnumerator Cr()
        {
            var duration = 0.25f;
            var curve = Curves.EaseInBack;
            var start = Data.Scale;
            yield return LerpEnumerator.Lerp01(duration, f =>
            {
                var t = curve.Evaluate(f);
                var scale = Mathf.Lerp(start, 0f, t);
                Scale = Vector3.One * scale;
            });

            QueueFree();
        }
    }
}

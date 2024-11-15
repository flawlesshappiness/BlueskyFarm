using Godot;
using System;
using System.Collections;

public partial class Item : Grabbable
{
    [Export]
    public int MaxUses;

    [NodeName]
    public Node3D ScaleNode;

    public ItemInfo Info { get; set; }
    public ItemData Data { get; set; }
    public bool IsBeingHandled { get; set; } // If the item is currently being handled by a script, other scripts will ignore this item
    public new Vector3 GlobalPosition { get => RigidBody.GlobalPosition; set => RigidBody.GlobalPosition = value; }
    public new Vector3 GlobalRotation { get => RigidBody.GlobalRotation; set => RigidBody.GlobalRotation = value; }
    public new Vector3 Scale { get => ScaleNode.Scale; set => ScaleNode.Scale = value; }

    public event Action OnUpdateData;
    public event Action OnAddedToInventory;

    public override void _Ready()
    {
        base._Ready();
        RigidBody.BodyEntered += OnBodyEntered;

        RigidBody.ContactMonitor = true;
        RigidBody.MaxContactsReported = 1;

        if (!OverrideInitialCollisionMode)
        {
            SetCollision_Item();
        }
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (RigidBody.GlobalPosition.Y < -50)
        {
            var bounds = FarmBounds.Instance;
            if (bounds != null)
            {
                bounds.ThrowObject(RigidBody, Player.Instance.GlobalPosition);
            }
        }
    }

    public void UpdateData()
    {
        var p = RigidBody.GlobalPosition;
        Data.X_Position = p.X;
        Data.Y_Position = p.Y;
        Data.Z_Position = p.Z;

        var r = RigidBody.GlobalRotation;
        Data.X_Rotation = r.X;
        Data.Y_Rotation = r.Y;
        Data.Z_Rotation = r.Z;

        OnUpdateData?.Invoke();
    }

    public void LoadFromData()
    {
        RigidBody.GlobalPosition = new Vector3(Data.X_Position, Data.Y_Position, Data.Z_Position);
        RigidBody.GlobalRotation = new Vector3(Data.X_Rotation, Data.Y_Rotation, Data.Z_Rotation);
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
        var vel = RigidBody.LinearVelocity;
        var vel_avg = (vel + vel_other) * avg_mul;

        if (vel_avg.Length() < vel_min) return;

        var t_vel = Mathf.Clamp((vel_avg.Length() - vel_min) / (vel_max - vel_min), 0, 1);
        var volume = Mathf.Lerp(vol_min, vol_max, t_vel);
        var pitch = Mathf.Lerp(pitch_max, pitch_min, t_vel);

        var asp = SoundController.Instance.Play("sfx_impact_default", RigidBody.GlobalPosition);
        asp.VolumeDb = volume;
        asp.PitchScale = pitch;
    }

    public virtual void Use()
    {
        // Use item
    }

    public void ReplenishUses(int uses)
    {
        Data.Uses = Mathf.Clamp(Data.Uses + uses, 0, MaxUses);
    }

    public virtual void AddToInventory()
    {
        InventoryController.Instance.Add(this);
        OnAddedToInventory?.Invoke();
    }

    public void ResetBodyPosition()
    {
        Body.Position = Vector3.Zero;
    }

    public void ResetBodyRotation()
    {
        Body.Rotation = Vector3.Zero;
    }

    public void LockPosition(bool x = false, bool y = false, bool z = false)
    {
        RigidBody.AxisLockLinearX = x;
        RigidBody.AxisLockLinearY = y;
        RigidBody.AxisLockLinearZ = z;
    }

    public void LockPosition_All() => LockPosition(x: true, y: true, z: true);
    public void UnlockPosition_All() => LockPosition();

    public void LockRotation(bool x = false, bool y = false, bool z = false)
    {
        RigidBody.AxisLockAngularX = x;
        RigidBody.AxisLockAngularY = y;
        RigidBody.AxisLockAngularZ = z;
    }

    public void LockRotation_All() => LockRotation(x: true, y: true, z: true);
    public void UnlockRotation_All() => LockRotation();

    public void SetScale(float scale)
    {
        Data.Scale = scale;
        Scale = Vector3.One * scale;
    }

    public Coroutine AnimateWobble()
    {
        return StartCoroutine(Cr, "animate");
        IEnumerator Cr()
        {
            var duration = 0.6f;
            var curve = AnimationCurves.WobbleOut(0.4f, 3);
            yield return LerpEnumerator.Lerp01(duration, f =>
            {
                var t = curve.Sample(f);
                ScaleNode.Scale = Vector3.One * (Data.Scale * t);
            });
        }
    }
}

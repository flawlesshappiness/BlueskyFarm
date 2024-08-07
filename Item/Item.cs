using Godot;
using System;

public partial class Item : Grabbable
{
    public ItemInfo Info { get; set; }
    public ItemData Data { get; set; }
    public bool IsBeingHandled { get; set; } // If the item is currently being handled by a script, other scripts will ignore this item

    public event Action OnUpdateData;
    public event Action OnAddedToInventory;

    public override void _Ready()
    {
        base._Ready();
        BodyEntered += OnBodyEntered;

        ContactMonitor = true;
        MaxContactsReported = 1;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (GlobalPosition.Y < -50)
        {
            FarmBounds.Instance.ThrowObject(this, FirstPersonController.Instance.GlobalPosition);
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

    public void LoadFromData()
    {
        GlobalPosition = new Vector3(Data.X_Position, Data.Y_Position, Data.Z_Position);
        GlobalRotation = new Vector3(Data.X_Rotation, Data.Y_Rotation, Data.Z_Rotation);
    }

    private void OnBodyEntered(Node other)
    {
        var rig_other = other as RigidBody3D;
        var vel_other = -rig_other?.LinearVelocity ?? Vector3.Zero;
        var avg_mul = rig_other == null ? 1 : 0.5f;
        var vel = LinearVelocity;
        var vel_avg = (vel + vel_other) * avg_mul;

        var vel_min = 0f;
        var vel_max = 5f;
        var vol_min = 0f;
        var vol_max = 30f;
        var pitch_min = 0.5f;
        var pitch_max = 1.5f;

        var t_vel = Mathf.Clamp((vel_avg.Length() - vel_min) / (vel_max - vel_min), 0, 1);
        var volume = Mathf.Lerp(vol_min, vol_max, t_vel);
        var pitch = Mathf.Lerp(pitch_max, pitch_min, t_vel);

        SoundController.Instance.Play("sfx_impact_default", new SoundSettings3D
        {
            Position = GlobalPosition,
            PitchMin = pitch,
            PitchMax = pitch,
            Volume = volume,
            Bus = SoundBus.SFX
        });
    }

    public void AddedToInventory()
    {
        OnAddedToInventory?.Invoke();
    }
}

using Godot;

public partial class Item : Grabbable
{
    public ItemInfo Info { get; set; }
    public ItemInfo PlantInfo { get; set; }
    public ItemData Data { get; set; }
    public bool IsBeingHandled { get; set; } // If the item is currently being handled by a script, other scripts will ignore this item

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
    }

    public void LoadFromData()
    {
        GlobalPosition = new Vector3(Data.X_Position, Data.Y_Position, Data.Z_Position);
        GlobalRotation = new Vector3(Data.X_Rotation, Data.Y_Rotation, Data.Z_Rotation);
    }
}

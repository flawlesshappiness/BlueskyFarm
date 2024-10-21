using System;

public class ItemData
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string CustomId { get; set; }
    public string InfoPath { get; set; }
    public string PlantInfoPath { get; set; }
    public float X_Position { get; set; }
    public float Y_Position { get; set; }
    public float Z_Position { get; set; }
    public float X_Rotation { get; set; }
    public float Y_Rotation { get; set; }
    public float Z_Rotation { get; set; }
    public float Scale { get; set; } = 1f;

    // Plant
    public bool PlantIsHuge { get; set; }
}

using Godot;
using System;

public partial class PlantAreaTouchable : Touchable
{
    private PlantArea _plant_area;

    private Seed CurrentSeed => _plant_area.CurrentSeed;
    private Vector3 SeedPosition => _plant_area.SeedPosition.GlobalPosition;

    public override void _Ready()
    {
        base._Ready();
        _plant_area = this.GetNodeInParents<PlantArea>();
    }

    public override bool HandleCursor()
    {
        if (CurrentSeed == null)
        {
            return false;
        }
        else if (CurrentSeed.IsFinished)
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
                Position = SeedPosition,
                Text = end_date.ToString("mm':'ss")
            });
            return true;
        }
    }
}
